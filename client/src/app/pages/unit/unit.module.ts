import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { UnitPageRoutingModule } from './unit-routing.module';
import { UnitPage } from './unit.page';
import { MaterialExampleModule } from 'src/app/material.module';
// import { MatDatepickerModule } from '@angular/material/datepicker';
// import { MatNativeDateModule } from '@angular/material/core';

@NgModule({
  imports: [
    MaterialExampleModule,
    ReactiveFormsModule,
    CommonModule,
    FormsModule,
    IonicModule,
    UnitPageRoutingModule,
    // MatDatepickerModule,
    // MatNativeDateModule
  ],
  declarations: [UnitPage]
})
export class UnitPageModule {}
