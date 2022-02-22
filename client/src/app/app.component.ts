import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { filter } from 'rxjs/operators';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: 'app.component.html',
  styleUrls: ['app.component.scss'],
})
export class AppComponent {

  // enableTabs:boolean;

  constructor(
    private router:Router,
    // private authService:AuthService
  ) {
    // this.authService.isAuthenticated
    // .pipe(
    //   filter(val=>val!==null)
    // )
    // .subscribe(
    //   res => this.enableTabs = res
    // )

    this.router.navigateByUrl('/')
  }



}
