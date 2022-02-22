import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { mergeMap } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { IPetition } from '../models/petition';
import { EventService } from './event.service';

@Injectable({
  providedIn: 'root'
})
export class PrivateDataService {

  private url = `${environment.api_url}`

  constructor(
    private http:HttpClient,
    private router:Router,
    private event:EventService
  ) { }

  get(){
    this.http.get(`${this.url}/petition`).subscribe((petitions:IPetition[])=>{
      this.event.publish('petition', petitions);
    })
  }

  saveOrupdate(action:string, petition:IPetition){
    const saveOrupdate = (action === 'update') ? this.http.put(`${this.url}/petition`, petition) : this.http.post(`${this.url}/petition`, petition)
    saveOrupdate.pipe(
      mergeMap(_=>this.http.get(`${this.url}/petition`))
    ).
    subscribe((petitions:IPetition[])=>{
      this.event.publish('petition', petitions);
      this.router.navigateByUrl('/');
    })
  }

  remove(id:string){
    this.http.delete(`${this.url}/petition/${id}`).pipe(
      mergeMap(_=>this.http.get(`${this.url}/petition`))
    ).
    subscribe((petitions:IPetition[])=>{
      this.event.publish('petition', petitions);
    })
  }
}
