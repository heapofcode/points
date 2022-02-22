import { Component, Input, ViewEncapsulation } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { PopoverController } from '@ionic/angular';
import { IUnit } from 'src/app/core/models/petition';

@Component({
  selector: 'app-unit',
  templateUrl: './unit.page.html',
  styleUrls: ['./unit.page.scss'],
  encapsulation:ViewEncapsulation.None
})
export class UnitPage {

  @Input() unit:IUnit

  unitForm = new FormGroup({
    id: new FormControl(''),
    fullName: new FormControl('', Validators.required),
    position: new FormControl('', Validators.required),
    documentIdentity: new FormControl('', Validators.required),
    birthDay: new FormControl('', Validators.required),
    birthPlace: new FormControl('', Validators.required),
    homeAdress: new FormControl('', Validators.required)
  });

  constructor(
    private popoverController:PopoverController
  ) {
  }

  ionViewWillEnter(){
    if(this.unit){
      this.unitForm.patchValue(this.unit)
    }
  }

  close(){
    this.popoverController.dismiss()
  }

  save(){
    this.popoverController.dismiss(this.unitForm.value)
  }
}
