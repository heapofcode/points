import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { IGeneral, IPetitionType, IPoint } from '../models/petition';

const url = `${environment.api_url}`;

@Injectable({
  providedIn: 'root'
})
export class InitService {

  points:IPoint[];
  types:IPetitionType[];

  constructor(
    private http:HttpClient
  ) { }

  init(){
    return this.http.get(`${url}/general`).subscribe((general:IGeneral)=>
    {
      this.points = general.points;
      this.types = general.petitionTypes;
      console.log("🚀 ~ Initialize app")
    })
  }
}
