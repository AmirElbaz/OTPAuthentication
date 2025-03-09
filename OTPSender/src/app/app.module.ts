// app.module.ts
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { OTPSenderModule } from './otpsender/otpsender.module';
import { routes } from './app.routes';

@NgModule({
  declarations: [
    AppComponent
    // other components
  ],
  imports: [
    BrowserModule,
    ReactiveFormsModule,
    HttpClientModule,
    RouterModule.forRoot(routes),
    OTPSenderModule
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }