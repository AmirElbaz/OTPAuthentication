
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { OtpResponse } from '../Models/otp-response';
import { ValidateOtpDTO } from '../Models/validate-otp-dto';

@Injectable({
  providedIn: 'root'
})
export class OTPService {
 
  constructor(
    private http: HttpClient
  ) { 
    
  }
  baseUrl: string = 'http://localhost:5293/api/EmailOTP';

  checkRemainingGenerations(contact: string, stepId: number): Observable<{ 
    remainingGenerations: number, 
    blocked?: boolean, 
    blockExpiry?: string 
  }> {
    return this.http.get<{ 
      remainingGenerations: number, 
      blocked?: boolean, 
      blockExpiry?: string 
    }>(`${this.baseUrl}/RemainingGenerations/${contact}/${stepId}`);
  }

  validateOtp(payload: ValidateOtpDTO): Observable<OtpResponse> {
    return this.http.post<OtpResponse>(`${this.baseUrl}/validate-OTP`, payload);
  }

  resendOtp(payload: { stepId: number; contact: string }): Observable<OtpResponse> {
    return this.http.post<OtpResponse>(`${this.baseUrl}/Generate`, payload);
  }

  checkBlockStatus(contact: string, stepId: number): Observable<{
    blocked: boolean,
    blockExpiry?: string,
    remainingBlockTime?: number
  }> {
    return this.http.get<{
      blocked: boolean,
      blockExpiry?: string,
      remainingBlockTime?: number
    }>(`${this.baseUrl}/BlockStatus/${contact}/${stepId}`);
  }

  private handleError(error: HttpErrorResponse): Observable<never> {
    let errorMessage = 'An unknown error occurred!';
    if (error.error instanceof ErrorEvent) {
      errorMessage = `Error: ${error.error.message}`;
    } else {
      errorMessage = `Error Code: ${error.status}\nMessage: ${error.error.message}`;
    }
    console.error(errorMessage);
    return throwError(() => new Error(errorMessage));
  }
}