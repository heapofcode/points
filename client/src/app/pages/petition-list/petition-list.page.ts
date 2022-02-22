import { Component } from '@angular/core';
import { IPetition } from 'src/app/core/models/petition';
import { AuthService } from 'src/app/core/services/auth.service';
import { SelectionModel } from '@angular/cdk/collections';
import { MatTableDataSource } from '@angular/material/table';
import { NavigationExtras, Router } from '@angular/router';
import { EventService } from 'src/app/core/services/event.service';
import { PrivateDataService } from 'src/app/core/services/private-data.service';
import { Platform } from '@ionic/angular';
import { PublicDataService } from 'src/app/core/services/public-data.service';

@Component({
  selector: 'app-petition-list',
  templateUrl: 'petition-list.page.html',
  styleUrls: ['petition-list.page.scss'],
})
export class PetitionListPage {
  state: string = 'add';
  displayedColumns: string[];
  petitionSource: MatTableDataSource<IPetition> =
    new MatTableDataSource<IPetition>();
  selection = new SelectionModel<IPetition>(true, []);

  constructor(
    private event: EventService,
    private authService: AuthService,
    private router: Router,
    private privateDataService:PrivateDataService,
    private publicDataService:PublicDataService,
    private platform:Platform
  ) {

    this.platform.is('mobile') ? this.displayedColumns = ['select', 'date', 'title'] : this.displayedColumns = ['select', 'date', 'title', 'reason']

    this.event.subscribe('petition', (res) => {
      this.petitionSource.data = res;
      this.petitionSource._updateChangeSubscription();
    });

    this.selection.changed.subscribe((_) => {
      if (this.selection.selected.length > 1) {
        this.state = 'remove';
      } else if (this.selection.selected.length === 1) {
        this.state = 'edit';
      } else {
        this.state = 'add';
      }
    });
  }

  checkboxLabel(row?: IPetition): string {
    if (!row) {
      return `${this.isAllSelected() ? 'deselect' : 'select'} all`;
    }
    return `${this.selection.isSelected(row) ? 'deselect' : 'select'} row ${
      row.id + 1
    }`;
  }

  isAllSelected() {
    const numSelected = this.selection.selected.length;
    const numRows = this.petitionSource.data.length;
    return numSelected === numRows;
  }

  masterToggle() {
    if (this.isAllSelected()) {
      this.selection.clear();
      return;
    }

    this.selection.select(...this.petitionSource.data);
  }

  ionViewWillEnter() {
    this.privateDataService.get();
  }

  ionViewWillLeave(){
    this.selection.clear();
  }

  edit() {
    const _petition = this.selection.selected[0];
    const navigationExtras: NavigationExtras = {
      state: {
        petition: _petition,
      },
    };
    this.router.navigate(['petition'], navigationExtras);
  }

  remove() {
    if (this.selection.selected.length){
      const id = this.selection.selected[0].id;
      this.privateDataService.remove(id);
      this.selection.clear();
    }
  }

  copy() {}

  docs() {
    this.publicDataService.createDOCX(this.selection.selected[0].id);
  }

  logout() {
    this.event.destroy('petition');
    this.authService.logout();
  }

  searchbarChange(e: any) {
    const val = e.detail.value;
    this.petitionSource.filter = val.trim().toLowerCase();
  }
}
