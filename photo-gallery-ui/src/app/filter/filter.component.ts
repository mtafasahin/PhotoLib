import { Component, EventEmitter, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
@Component({
  selector: 'app-filter',
  standalone: true,
  templateUrl: './filter.component.html',
  imports: [FormsModule,CommonModule], // FormsModule dahil edildi
  styleUrls: ['./filter.component.scss']
})
export class FilterComponent {
  @Output() filtersChanged = new EventEmitter<{ year: string; month: string }>();

  filters = {
    year: '',
    month: ''
  };

  months = [
    { value: '1', label: 'January' },
    { value: '2', label: 'February' },
    { value: '3', label: 'March' },
    { value: '4', label: 'April' },
    { value: '5', label: 'May' },
    { value: '6', label: 'June' },
    { value: '7', label: 'July' },
    { value: '8', label: 'August' },
    { value: '9', label: 'September' },
    { value: '10', label: 'October' },
    { value: '11', label: 'November' },
    { value: '12', label: 'December' }
  ];

  applyFilters() {
    this.filtersChanged.emit(this.filters);
  }
}
