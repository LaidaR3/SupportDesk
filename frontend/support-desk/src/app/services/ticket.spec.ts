import { TestBed } from '@angular/core/testing';

import { Ticket } from './ticket';

describe('Ticket', () => {
  let service: Ticket;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Ticket);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
