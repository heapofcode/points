import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import * as FileSaver from 'file-saver';

const url = `${environment.api_url}`;

@Injectable({
  providedIn: 'root'
})

export class PublicDataService {
  constructor(
    private http:HttpClient
  ){ }

  createDOCX(id:string){
    this.http.get(`${url}/general/${id}`, { responseType: 'blob'}).subscribe((blob:any)=>{
      if(blob){
        FileSaver.saveAs(blob, 'file.docx')
      }
    })
  }
}
