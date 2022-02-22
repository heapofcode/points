import { IonicModule } from '@ionic/angular';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PetitionListPage } from './petition-list.page';
import { PetitionListPageRoutingModule } from './petition-list-routing.module';
import { MaterialExampleModule } from 'src/app/material.module';

@NgModule({
  imports: [
    MaterialExampleModule,
    IonicModule,
    CommonModule,
    PetitionListPageRoutingModule,
    FormsModule,
    ReactiveFormsModule,
  ],
  declarations: [PetitionListPage],
})
export class PetitionListPageModule {}
