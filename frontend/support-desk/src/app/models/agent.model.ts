export type Department = 'Technical' | 'Billing' | 'General';

export interface Agent {
  id: number;
  fullName: string;
  email: string;
  department: Department;
  active: boolean;
}