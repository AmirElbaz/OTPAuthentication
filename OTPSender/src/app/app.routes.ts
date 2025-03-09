import { Routes } from '@angular/router';
import { OTPHandlerComponent } from './otpsender/components/otphandler/otphandler.component';

export const routes: Routes = [
    { path: 'otp-validation/:stepId/:contact', component: OTPHandlerComponent },

];
