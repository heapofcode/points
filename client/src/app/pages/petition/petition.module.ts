import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { PetitionPageRoutingModule } from './petition-routing.module';
import { PetitionPage } from './petition.page';
import { MaterialExampleModule } from 'src/app/material.module';

@NgModule({
  imports: [
    MaterialExampleModule,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    IonicModule,
    PetitionPageRoutingModule,
  ],
  declarations: [PetitionPage],
})
export class PetitionPageModule {}
