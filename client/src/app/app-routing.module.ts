import { NgModule } from '@angular/core';
import { PreloadAllModules, RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';

const routes: Routes = [
  {
    path: '',
    redirectTo: 'petition-list',
    pathMatch: 'full',
  },
  {
    path: 'petition-list',
    loadChildren: () => import('./pages/petition-list/petition-list.module').then(m => m.PetitionListPageModule),
    canLoad:[AuthGuard]
  },
  {
    path: 'petition',
    loadChildren: () => import('./pages/petition/petition.module').then( m => m.PetitionPageModule),
    canLoad:[AuthGuard]
  },
  {
    path: 'unit',
    loadChildren: () => import('./pages/unit/unit.module').then(m => m.UnitPageModule),
  },
  {
    path: 'login',
    loadChildren: () => import('./pages/login/login.module').then(m => m.LoginPageModule),
  },
];
@NgModule({
  imports: [
    RouterModule.forRoot(routes, { preloadingStrategy: PreloadAllModules })
  ],
  exports: [RouterModule]
})
export class AppRoutingModule {}
