import { Component, ViewEncapsulation } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { IPetition, IUnit } from 'src/app/core/models/petition';
import { MatTableDataSource } from '@angular/material/table';
import { SelectionModel } from '@angular/cdk/collections';
import { UnitService } from 'src/app/core/services/unit.service';
import { PrivateDataService } from 'src/app/core/services/private-data.service';
import { Router } from '@angular/router';
import { InitService } from 'src/app/core/services/init.service';

@Component({
  selector: 'app-petition',
  templateUrl: './petition.page.html',
  styleUrls: ['./petition.page.scss'],
  encapsulation:ViewEncapsulation.None
})
export class PetitionPage {

  petitionForm:FormGroup = new FormGroup({
    id: new FormControl(''),
    title: new FormControl('', Validators.required),
    reason: new FormControl('', Validators.required),
    dateFrom: new FormControl('', Validators.required),
    dateTo: new FormControl('', Validators.required),
    vehicle: new FormControl(''),
    type: new FormControl('', Validators.required),
    points: new FormControl([], Validators.required),
    units: new FormControl([], Validators.required)
  });

  action:string = 'save';
  stateUnits:string = 'add';
  displayedColumns: string[] = ['select', 'fullName', 'position', 'documentIdentity'];
  unitSource: MatTableDataSource<IUnit> = new MatTableDataSource<IUnit>([] as IUnit[]);
  selection = new SelectionModel<IUnit>(true, []);

  constructor(
    private router:Router,
    private unitService:UnitService,
    private privateDataSerivce:PrivateDataService,
    public publicData:InitService
  ) {

    if(this.router.getCurrentNavigation().extras.state){
      const petition:IPetition = this.router.getCurrentNavigation().extras.state.petition
      this.action = 'update';
      this.unitSource.data = petition.units;
      this.unitSource._updateChangeSubscription();
      this.petitionForm.patchValue(petition);
    }

    this.petitionForm.get('units').valueChanges.subscribe(units=>{
      this.unitSource.data = units;
      this.unitSource._updateChangeSubscription();
    })

    this.selection.changed.subscribe(_ => {
      if (this.selection.selected.length > 1) {
        this.stateUnits = 'remove';
      } else if (this.selection.selected.length === 1) {
        this.stateUnits = 'edit';
      } else {
        this.stateUnits = 'add';
      }
    });

  }

  async unitActions(action:string){
    var _units:IUnit[] = [];

    if(action==='add'){
      _units = await this.unitService.add(this.petitionForm.get('units').value)
    }else if (action==='edit' && this.selection.selected.length>0){
      _units = await this.unitService.edit(this.petitionForm.get('units').value, this.selection.selected[0])
    }else if (action==='remove' && this.selection.selected.length>0){
      _units = await this.unitService.remove(this.petitionForm.get('units').value, this.selection.selected)
    }

    if(_units){
      this.petitionForm.patchValue({
        units:_units
      });
    }

    this.selection.clear();
  }

  save(){
    this.privateDataSerivce.saveOrupdate(this.action, this.petitionForm.value)
  }

  comparePoints(object1: any, object2: any){
    return object1 && object2 && object1.title == object2.title;
  }

  compareTypes(object1: any, object2: any){
    return object1 && object2 && object1 == object2;
  }

  checkboxLabel(row?: IUnit): string {
    if (!row) {
      return `${this.isAllSelected() ? 'deselect' : 'select'} all`;
    }
    return `${this.selection.isSelected(row) ? 'deselect' : 'select'} row ${
      row.fullName + 1
    }`;
  }

  isAllSelected() {
    const numSelected = this.selection.selected.length;
    const numRows = this.unitSource.data.length;
    return numSelected === numRows;
  }

  masterToggle() {
    if (this.isAllSelected()) {
      this.selection.clear();
      return;
    }

    this.selection.select(...this.unitSource.data);
  }
}
