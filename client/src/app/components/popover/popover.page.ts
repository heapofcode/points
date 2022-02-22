import { Component, Input } from '@angular/core';
import { PopoverController } from '@ionic/angular';

@Component({
  selector: 'app-popover',
  templateUrl: './popover.page.html',
  styleUrls: ['./popover.page.scss'],
})
export class PopoverPage {

  value:any;

  @Input() list:[]
  @Input() title:string

  constructor(
    private popoverController:PopoverController
  ) {
    console.log(this.list)
  }

  getValue(ev:any){
    this.value = ev.detail.value;
  }

  accept(){
    this.popoverController.dismiss(this.value)
  }

  cancel(){
    this.popoverController.dismiss(undefined)
  }

}
