import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { ViewanyPage } from './viewany.page';

const routes: Routes = [
  {
    path: '',
    component: ViewanyPage
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ViewanyPageRoutingModule {}
