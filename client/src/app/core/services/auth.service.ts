import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { tap, switchMap } from 'rxjs/operators';
import { BehaviorSubject, from, Observable, of } from 'rxjs';
import { Storage } from '@capacitor/storage';
import { Router } from '@angular/router';
import { environment } from 'src/environments/environment';

const ACCESS_TOKEN_KEY = 'accessToken';
const REFRESH_TOKEN_KEY = 'refreshToken';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  // Init with null to filter out the first value in a guard!

  private roles:BehaviorSubject<any> = new BehaviorSubject<any>(null);
  roles$:Observable<any> = this.roles.asObservable();

  isAuthenticated: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(null);
  currentAccessToken = null;
  url = `${environment.api_url}/account`;

  constructor(
    private http: HttpClient,
    private router: Router)
  {
    this.loadToken();
    //this._data.next([{id:1, foo:2}]);
  }

  // Load accessToken on startup
  async loadToken() {
    const token = await Storage.get({ key: ACCESS_TOKEN_KEY });
    if (token && token.value) {
      this.currentAccessToken = token.value;
      this.isAuthenticated.next(true);
    } else {
      this.isAuthenticated.next(false);
    }
  }

  // Get our secret protected data
  getSecretData() {
    return this.http.get(`${this.url}/user`).subscribe(
      (data:any) => {
        this.roles.next(data.roles)
      }
    )
  }

  // Create new user
  signUp(credentials: {username, password}): Observable<any> {
    return this.http.post(`${this.url}/users`, credentials);
  }

  // Sign in a user and store access and refres token
  login(credentials: {username, password}): Observable<any> {
    return this.http.post(`${this.url}/login`, credentials).pipe(
      switchMap((tokens: {accessToken, refreshToken, roles }) => {
        this.roles.next(tokens.roles)
        this.currentAccessToken = tokens.accessToken;
        const storeAccess = Storage.set({key: ACCESS_TOKEN_KEY, value: tokens.accessToken});
        const storeRefresh = Storage.set({key: REFRESH_TOKEN_KEY, value: tokens.refreshToken});
        return from(Promise.all([storeAccess, storeRefresh]));
      }),
      tap(_ => {
        this.isAuthenticated.next(true);
      })
    )
  }

  // Potentially perform a logout operation inside your API
  // or simply remove all local tokens and navigate to login
  logout() {
    return this.http.post(`${this.url}/logout`, {}).pipe(
      switchMap(_ => {
        this.currentAccessToken = null;
        // Remove all stored tokens
        const deleteAccess = Storage.remove({ key: ACCESS_TOKEN_KEY });
        const deleteRefresh = Storage.remove({ key: REFRESH_TOKEN_KEY });
        return from(Promise.all([deleteAccess, deleteRefresh]));
      }),
      tap(_ => {
        this.isAuthenticated.next(false);
        this.router.navigateByUrl('/login', { replaceUrl: true });
      })
    ).subscribe();
  }

  // Load the refresh token from storage
  // then attach it as the header for one specific API call
  getNewAccessToken() {
    const refreshToken = from(Storage.get({ key: REFRESH_TOKEN_KEY }));
    return refreshToken.pipe(
      switchMap(token => {
        if (token && token.value) {
          const httpOptions = {
            headers: new HttpHeaders({
              'Content-Type': 'application/json',
              Authorization: `${token.value}`
            })
          }
          return this.http.get(`${this.url}/refresh`, httpOptions);
        } else {
          // No stored refresh token
          return of(null);
        }
      })
    );
  }

  // Store a new access token
  storeAccessToken(token) {
    this.currentAccessToken = token.accessToken;
    Storage.set({ key: REFRESH_TOKEN_KEY, value: token.refreshToken });
    return from(Storage.set({ key: ACCESS_TOKEN_KEY, value: token.accessToken }));
  }

}
