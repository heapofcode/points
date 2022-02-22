import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PetitionListPage } from './petition-list.page';

const routes: Routes = [
  {
    path: '',
    component: PetitionListPage,
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PetitionListPageRoutingModule {}
