import { Injectable } from '@angular/core';
import { PopoverController } from '@ionic/angular';
import { UnitPage } from 'src/app/pages/unit/unit.page';
import { IUnit } from '../models/petition';

@Injectable({
  providedIn: 'root',
})
export class UnitService {
  constructor(private popoverController: PopoverController) {}

  async add(units: IUnit[]) {
    const unitPopover = await this.popoverController.create({
      component: UnitPage,
      cssClass: 'unit-popover',
    });
    await unitPopover.present();
    const { data } = await unitPopover.onDidDismiss();

    if (data) {
      units.push(data);
      return units;
    }
  }

  async edit(units: IUnit[], editUnit: IUnit) {
    const unitPopover = await this.popoverController.create({
      component: UnitPage,
      componentProps: {
        unit: editUnit,
      },
      cssClass: 'unit-popover',
    });
    await unitPopover.present();
    const { data } = await unitPopover.onDidDismiss();

    if (data) {
      const index = units.indexOf(editUnit);
      units[index] = data;
      return units;
    }
  }

  async remove(units: IUnit[], removeUnits: IUnit[]) {
    var _units = units.filter((x) => !removeUnits.includes(x));
    return _units;
  }
}
