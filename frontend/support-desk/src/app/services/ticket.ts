import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import {
  CreateTicket,
  PagedResult,
  Ticket,
  TicketPriority,
  TicketStatus,
  UpdateTicket
} from '../models/ticket.model';

import { TicketComment } from '../models/comment.model';

@Injectable({
  providedIn: 'root'
})
export class TicketService {
  private readonly apiUrl = 'http://localhost:5002/api/tickets';

  constructor(private http: HttpClient) {}

  getTickets(
    page = 1,
    pageSize = 10,
    search = '',
    status?: TicketStatus,
    priority?: TicketPriority,
    agentId?: number,
    overdue?: boolean
  ): Observable<PagedResult<Ticket>> {
    let params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    if (search.trim()) {
      params = params.set('search', search.trim());
    }

    if (status) {
      params = params.set('status', status);
    }

    if (priority) {
      params = params.set('priority', priority);
    }

    if (agentId) {
      params = params.set('agentId', agentId);
    }

    if (overdue === true) {
      params = params.set('overdue', true);
    }

    return this.http.get<PagedResult<Ticket>>(this.apiUrl, { params });
  }

  getTicket(id: number): Observable<Ticket> {
    return this.http.get<Ticket>(`${this.apiUrl}/${id}`);
  }

  createTicket(ticket: CreateTicket): Observable<Ticket> {
    return this.http.post<Ticket>(this.apiUrl, ticket);
  }

  updateTicket(id: number, ticket: UpdateTicket): Observable<Ticket> {
    return this.http.put<Ticket>(`${this.apiUrl}/${id}`, ticket);
  }

  deleteTicket(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  assignAgent(id: number, agentId: number | null): Observable<Ticket> {
    return this.http.put<Ticket>(
      `${this.apiUrl}/${id}/assignment`,
      { agentId }
    );
  }

  changeStatus(id: number, status: TicketStatus): Observable<Ticket> {
    return this.http.put<Ticket>(
      `${this.apiUrl}/${id}/status`,
      { status }
    );
  }

  addComment(
    id: number,
    authorName: string,
    body: string
  ): Observable<TicketComment> {
    return this.http.post<TicketComment>(
      `${this.apiUrl}/${id}/comments`,
      { authorName, body }
    );
  }
}