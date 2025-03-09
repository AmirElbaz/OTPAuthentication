import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common'; // ✅ Import this

@Component({
  selector: 'app-steps-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './steps-list.component.html',
  styleUrl: './steps-list.component.css'
})
export class StepsListComponent implements OnInit {
 
  steps: Step[] = [];
  apiUrl = 'http://localhost:5214/api/StepAuthenticator';
  constructor(private http: HttpClient) {
    
  }
  ngOnInit() {
    this.fetchSteps();
  }

  fetchSteps() {
    this.http.get<Step[]>(this.apiUrl).subscribe({
      next: (data) => {
        this.steps = data;
      },
      error: (err) => {
        console.error('Failed to fetch steps:', err);
        alert('Failed to load steps.');
      }
    });
  }
 authenticate(stepId: number) {
  const request: AuthRequestDTO = {
    stepId: stepId,
    method: 1 // Assuming 1 = Email authentication
  };

  const apiUrl = 'http://localhost:5214/api/StepAuthenticator/authenticate';
  this.http.post<{ redirectUrl: string }>(apiUrl, request).subscribe({
    next: (response) => {
      console.log(response.redirectUrl);
      if (response.redirectUrl) {
      window.location.href = response.redirectUrl; // ✅ Redirect in Angular
    }},
    error: () => alert('Authentication failed. Please try again.')
  });
  
}
}
export enum StepStatus {
  Pending = 'Pending',
  InProgress = 'In Progress',
  Completed = 'Completed'
}

export interface Step {
  id: number;
  status: StepStatus;
  responsibleUserId: number;
}
export interface AuthRequestDTO {
  stepId: number;
  method: number; 
}