import { Component, Input, ViewChild } from '@angular/core';
import { SwiperOptions } from 'swiper';
import SwiperCore, { Zoom } from 'swiper';
import { ModalController } from '@ionic/angular';
import { SwiperComponent } from 'swiper/angular';
SwiperCore.use([Zoom])

const PDF_DEF_ZOOM = 1;
const PDF_ZOOM_STEP = 0.25;

@Component({
  selector: 'app-viewany',
  templateUrl: './viewany.page.html',
  styleUrls: ['./viewany.page.scss'],
})
export class ViewanyPage {

  @ViewChild('swiper') swiper:SwiperComponent

  // @Input() file:File
  @Input() type:string;
  @Input() src:string;

  // src:string = "";

  pdfZoom:number = PDF_DEF_ZOOM;
  swiperOptions:SwiperOptions = {
    zoom:true
  }

  constructor(
    private modalController:ModalController
  ) { }

  // async ngOnInit() {
  //   this.src = await this.getBase64(this.file) as string;
  // }

  close(){
    this.modalController.dismiss();
  }

  async zoom(zoomIn: boolean){
    if(this.type !== "application/pdf"){
      const zoom = this.swiper.swiperRef.zoom;
      zoomIn ? zoom.in() : zoom.out()
    }else{
      zoomIn ? this.pdfZoom += PDF_ZOOM_STEP : this.pdfZoom -= PDF_ZOOM_STEP
    }
  }
}
