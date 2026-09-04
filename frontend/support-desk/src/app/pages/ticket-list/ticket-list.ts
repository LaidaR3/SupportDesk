import {
  ChangeDetectorRef,
  Component,
  OnInit
} from '@angular/core';

import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { TicketService } from '../../services/ticket';

import {
  Ticket,
  TicketPriority,
  TicketStatus
} from '../../models/ticket.model';

@Component({
  selector: 'app-ticket-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ticket-list.html',
  styleUrl: './ticket-list.css'
})
export class TicketList implements OnInit {
  tickets: Ticket[] = [];

  page = 1;
  pageSize = 10;
  totalPages = 0;
  totalCount = 0;

  search = '';
  status: TicketStatus | null = null;
  priority: TicketPriority | null = null;
  overdue = false;

  loading = false;
  errorMessage = '';

  statuses: TicketStatus[] = [
    'New',
    'InProgress',
    'Resolved',
    'Closed'
  ];

  priorities: TicketPriority[] = [
    'Low',
    'Normal',
    'High',
    'Critical'
  ];

  constructor(
    private ticketService: TicketService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadTickets();
  }

  loadTickets(): void {
    this.loading = true;
    this.errorMessage = '';

    this.ticketService
      .getTickets(
        this.page,
        this.pageSize,
        this.search,
        this.status ?? undefined,
        this.priority ?? undefined,
        undefined,
        this.overdue
      )
      .subscribe({
        next: response => {
          this.tickets = response.items;
          this.totalPages = response.totalPages;
          this.totalCount = response.totalCount;
          this.loading = false;

          this.cdr.markForCheck();
        },
        error: () => {
          this.errorMessage = 'Failed to load tickets.';
          this.loading = false;

          this.cdr.markForCheck();
        }
      });
  }

  applyFilters(): void {
    this.page = 1;
    this.loadTickets();
  }

  clearFilters(): void {
    this.search = '';
    this.status = null;
    this.priority = null;
    this.overdue = false;
    this.page = 1;

    this.loadTickets();
  }

  previousPage(): void {
    if (this.page > 1) {
      this.page--;
      this.loadTickets();
    }
  }

  nextPage(): void {
    if (this.page < this.totalPages) {
      this.page++;
      this.loadTickets();
    }
  }
}