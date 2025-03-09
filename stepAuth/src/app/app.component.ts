import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { StepsListComponent } from "./steps-list/steps-list.component";

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, StepsListComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'stepAuth';
}
