import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { provideHttpClient } from '@angular/common/http';
import { ReactiveFormsModule } from '@angular/forms'; // Import ReactiveFormsModule here
import { OTPHandlerComponent } from './components/otphandler/otphandler.component';

@NgModule({
  declarations: [
    OTPHandlerComponent 
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule 
  ],
  providers: [provideHttpClient()]
})
export class OTPSenderModule { }