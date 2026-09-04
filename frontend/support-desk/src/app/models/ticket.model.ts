import { Agent } from './agent.model';
import { TicketComment } from './comment.model';

export type TicketPriority =
  | 'Low'
  | 'Normal'
  | 'High'
  | 'Critical';

export type TicketStatus =
  | 'New'
  | 'InProgress'
  | 'Resolved'
  | 'Closed';

export interface Ticket {
  id: number;
  reference: string;
  title: string;
  description: string;
  customerName: string;
  customerEmail: string;
  priority: TicketPriority;
  status: TicketStatus;
  assignedAgent: Agent | null;
  createdDate: string;
  lastModifiedDate: string;
  resolvedDate: string | null;
  closedDate: string | null;
  dueDate: string;
  isOverdue: boolean;
  comments: TicketComment[];
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface CreateTicket {
  title: string;
  description: string;
  customerName: string;
  customerEmail: string;
  priority: TicketPriority;
}

export interface UpdateTicket extends CreateTicket {}