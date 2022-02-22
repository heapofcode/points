export interface IPetition {
  id:string
  title:string
  reason:string
  vehicle:string
  type:string
  checked:boolean
  date:Date
  dateFrom:Date
  dateTo:Date
  units:IUnit[]
  points:IPoint[]
}

export interface IUnit{
  id:string
  fullName:string
  position:string
  documentIdentity:string
  birthDay:Date
  birthPlace:string
  homeAdress:string
}

export interface IPoint{
  id:string
  title:string
}

export interface IPetitionType{
  id:string
  title:string
}

export interface IGeneral{
  points:IPoint[]
  petitionTypes:IPetitionType[]
}

export interface IPointChief {
  title:string
  chiefFullName:string
  phoneNumber:string
  address:string
}

export interface IDoc {
  contentType:string;
  fileContent:any;
}
