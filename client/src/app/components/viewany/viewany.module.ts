import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { IonicModule } from '@ionic/angular';

import { ViewanyPageRoutingModule } from './viewany-routing.module';

import { SwiperModule } from 'swiper/angular'
import { ViewanyPage } from './viewany.page';
import { PdfViewerModule } from 'ng2-pdf-viewer';

@NgModule({
  imports: [
    SwiperModule,
    PdfViewerModule,
    CommonModule,
    FormsModule,
    IonicModule,
    ViewanyPageRoutingModule
  ],
  declarations: [ViewanyPage]
})
export class ViewanyPageModule {}
