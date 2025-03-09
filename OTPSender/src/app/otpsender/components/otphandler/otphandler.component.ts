
import { Component } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { OTPService } from '../../Services/otp.service';
import { ValidateOtpDTO } from '../../Models/validate-otp-dto';
import { finalize } from 'rxjs';
import { OtpResponse } from '../../Models/otp-response';

@Component({
  selector: 'app-otphandler',
  templateUrl: './otphandler.component.html',
  styleUrl: './otphandler.component.css'
})
export class OTPHandlerComponent {
  stepId: number = 0;
  
  otpDigits: number[] = [1, 2, 3, 4, 5, 6]; // 6-digit OTP
  otpControls: FormControl[] = [];
  errorMessage?: string = '';
  loading: boolean = false;
  timerCount: number = 0;
  timerInterval: any;
  
  // OTP state tracking
  remainingGenerations: number = 3;
  attemptsLeft: number = 3;
  otpBlocked: boolean = false;
  otpExpired: boolean = false;
  contact: string = '';

  // Block status tracking
  blockExpiry: Date | null = null;
  blockCountdownInterval: any;
  blockTimeRemaining: string = '';
  
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private otpService: OTPService
  ) {
    // Initialize form controls for each OTP digit
    this.otpDigits.forEach(() => {
      this.otpControls.push(new FormControl('', [
        Validators.required,
        Validators.maxLength(1),
        Validators.pattern('[0-9]')
      ]));
    });
  }
  
  ngOnInit(): void {
    // Extract parameters from URL
    this.route.params.subscribe(params => {
      this.stepId = +params['stepId']; // Convert to number with + operator
      this.contact = params['contact'];
      if (!this.stepId || !this.contact) {
        this.errorMessage = 'Invalid URL parameters';
        return;
      }

      // Check remaining OTP generations and block status
      this.checkOtpStatus();
    });
  }

  checkOtpStatus(): void {
    // First check if the user is blocked
    this.otpService.checkBlockStatus(this.contact, this.stepId)
      .subscribe({
        next: (response) => {
          if (response.blocked) {
            this.handleBlockedStatus(response.blockExpiry);
          } else {
            // If not blocked, check remaining generations
            this.checkRemainingGenerations();
          }
        },
        error: () => {
          this.errorMessage = 'Failed to check block status';
        }
      });
  }

  checkRemainingGenerations(): void {
    this.otpService.checkRemainingGenerations(this.contact, this.stepId)
      .subscribe({
        next: (response) => {
          this.remainingGenerations = response.remainingGenerations;
          
          if (response.blocked) {
            this.handleBlockedStatus(response.blockExpiry);
          } else if (this.remainingGenerations <= 0) {
            this.otpBlocked = true;
            this.errorMessage = 'Maximum OTP generation limit reached for this step';
          }
        },
        error: () => {
          this.errorMessage = 'Failed to check OTP status';
        }
      });
  }

  handleBlockedStatus(blockExpiryStr?: string): void {
    this.otpBlocked = true;
    
    if (blockExpiryStr) {
      this.blockExpiry = new Date(blockExpiryStr);
      this.startBlockCountdown();
    }
  }

  startBlockCountdown(): void {
    if (!this.blockExpiry) return;
    
    // Clear any existing interval
    if (this.blockCountdownInterval) {
      clearInterval(this.blockCountdownInterval);
    }
    
    // Update immediately
    this.updateBlockTimeRemaining();
    
    // Then set interval
    this.blockCountdownInterval = setInterval(() => {
      const unblocked = this.updateBlockTimeRemaining();
      if (unblocked) {
        clearInterval(this.blockCountdownInterval);
        this.resetAfterBlockExpiry();
      }
    }, 1000);
  }

  updateBlockTimeRemaining(): boolean {
    if (!this.blockExpiry) return false;
    
    const now = new Date();
    const diff = this.blockExpiry.getTime() - now.getTime();
    
    if (diff <= 0) {
      this.blockTimeRemaining = 'Unblocked';
      return true; // Block expired
    }
    
    // Format remaining time
    const minutes = Math.floor(diff / 60000);
    const seconds = Math.floor((diff % 60000) / 1000);
    this.blockTimeRemaining = `${minutes}m ${seconds}s`;
    return false; // Still blocked
  }

  resetAfterBlockExpiry(): void {
    // Reset block status
    this.otpBlocked = false;
    this.blockExpiry = null;
    this.errorMessage = '';
    
    // Reset attempts counter
    this.attemptsLeft = 3;
    
    // Check new remaining generations
    this.checkRemainingGenerations();
  }

  onOtpDigitInput(event: any, index: number): void {
    const input = event.target;
    const value = input.value;
    
    // Move to next input if current field is filled
    if (value.length === 1 && index < this.otpDigits.length - 1) {
      const nextInput = input.nextElementSibling;
      if (nextInput) {
        nextInput.focus();
      }
    }
    
    // Handle backspace to move to previous input
    if (event.key === 'Backspace' && index > 0 && !value) {
      const prevInput = input.previousElementSibling;
      if (prevInput) {
        prevInput.focus();
      }
    }
  }

  onPaste(event: ClipboardEvent): void {
    event.preventDefault();
    
    if (!event.clipboardData) return;
    
    const pastedData = event.clipboardData.getData('text');
    const digits = pastedData.replace(/\D/g, '').substring(0, this.otpDigits.length).split('');
    
    if (digits.length > 0) {
      digits.forEach((digit, index) => {
        if (index < this.otpControls.length) {
          this.otpControls[index].setValue(digit);
        }
      });
      
      // Focus on the next empty input or the last input if all are filled
      const nextEmptyIndex = this.otpControls.findIndex(control => !control.value);
      const focusIndex = nextEmptyIndex !== -1 ? nextEmptyIndex : this.otpControls.length - 1;
      
      const inputs = document.querySelectorAll('.otp-input');
      if (inputs[focusIndex]) {
        (inputs[focusIndex] as HTMLElement).focus();
      }
    }
  }

  isOtpComplete(): boolean {
    return this.otpControls.every(control => control.valid && control.value !== '');
  }

  getOtpValue(): string {
    return this.otpControls.map(control => control.value).join('');
  }

  validateOtp(decision: number): void {
    if (!this.isOtpComplete() && decision === 1) {
      this.errorMessage = 'Please enter the complete OTP';
      return;
    }
    
    this.loading = true;
    this.errorMessage = '';
    
    const payload: ValidateOtpDTO = {
      stepId: this.stepId,
      otp: this.getOtpValue(),
      decision: decision,
      contact: this.contact
    };
    console.log(payload);
    this.otpService.validateOtp(payload)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: (response) => {
          if (response.success) {
            // Check if the response contains the AuthenticationURL
            if (response.authenticationURL) {
              // Redirect to the AuthenticationURL
              console.log(response.authenticationURL);
              window.location.href = response.authenticationURL;
            } else {
              // Fallback to a default URL if AuthenticationURL is not provided
              window.location.href = 'https://www.google.com';
            }
          }
        },
        error: (error) => {
          if (error.status === 401 && error.error) {
            const response = error.error as OtpResponse;
            this.errorMessage = response.message;
            
            if (response.blocked) {
              this.handleBlockedStatus(response.blockExpiry);
              return;
            }
            
            if (response.maxAttemptsReached) {
              this.otpBlocked = true;
              if (response.blockExpiry) {
                this.handleBlockedStatus(response.blockExpiry);
              }
            }
            
            if (response.expired) {
              this.otpExpired = true;
            }
            
            if (response.attemptsLeft !== undefined) {
              this.attemptsLeft = response.attemptsLeft;
            }
          } else {
            this.errorMessage = 'An error occurred during validation. Please try again.';
          }
        }
      });
  }

  resendOtp(): void {
    if (this.timerCount > 0 || this.otpBlocked) return;
    
    this.loading = true;
    this.errorMessage = '';
    
    const payload = {
      stepId: this.stepId,
      contact: this.contact
    };
    
    this.otpService.resendOtp(payload)
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: (response) => {
          if (response.success) {
            // Start the timer
            this.startResendTimer();
            // Reset the form
            this.otpControls.forEach(control => control.reset());
            // Focus on the first input
            const inputs = document.querySelectorAll('.otp-input');
            if (inputs[0]) {
              (inputs[0] as HTMLElement).focus();
            }
            // Reset expired state
            this.otpExpired = false;
            // Update remaining generations
            this.remainingGenerations--;
            this.attemptsLeft = 3; // Reset attempts for new OTP
          }
        },
        error: (error) => {
          if (error.error) {
            this.errorMessage = error.error.message || 'Failed to resend OTP';
            
            if (error.error.blocked) {
              this.handleBlockedStatus(error.error.blockExpiry);
              return;
            }
            
            if (error.error.message?.includes('Maximum OTP generation limit')) {
              this.otpBlocked = true;
              this.remainingGenerations = 0;
            }
          } else {
            this.errorMessage = 'Failed to resend OTP. Please try again.';
          }
        }
      });
  }

  startResendTimer(): void {
    this.timerCount = 60;
    clearInterval(this.timerInterval);
    
    this.timerInterval = setInterval(() => {
      this.timerCount--;
      if (this.timerCount <= 0) {
        clearInterval(this.timerInterval);
      }
    }, 1000);
  }

  ngOnDestroy(): void {
    clearInterval(this.timerInterval);
    if (this.blockCountdownInterval) {
      clearInterval(this.blockCountdownInterval);
    }
  }
}