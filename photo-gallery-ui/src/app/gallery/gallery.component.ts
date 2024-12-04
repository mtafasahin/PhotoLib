import { Component } from '@angular/core';
import { HttpClientModule, HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatButtonModule } from '@angular/material/button';
import { FilterComponent } from '../filter/filter.component';
import {MatChipEditedEvent, MatChipInputEvent, MatChipsModule} from '@angular/material/chips';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {LiveAnnouncer} from '@angular/cdk/a11y';
import {COMMA, ENTER} from '@angular/cdk/keycodes';
import {ChangeDetectionStrategy, inject, signal} from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ImageDialogComponent } from '../image-dialog/image-dialog.component';
import { MatDialogModule } from '@angular/material/dialog';
import { LabelService } from '../services/label.service';

interface Image {
  id: number;
  url: string;
  thumbUrl: string;
  brand?: string;
  model?: string;
  takenDate?: string;
  labels?: string;
  originalUrl?: string;
  similarCount: number;
}

export interface Label {
  name: string;
  id: number;
}

@Component({
  selector: 'app-gallery',
  standalone: true,
  templateUrl: './gallery.component.html',
  styleUrls: ['./gallery.component.scss'],
  imports: [CommonModule, HttpClientModule, MatGridListModule, MatButtonModule,FilterComponent,
      MatFormFieldModule, MatChipsModule, MatIconModule,MatDialogModule], // HttpClientModule eklendi
})
export class GalleryComponent {
  images: Image[] = []; // Tip tanımlaması yapıldı
  filteredImages: Image[] = [...this.images];
  selectedImageIds: number[] = []; // Seçilen resimlerin ID'lerini tutar
  pageNumber: number = 1;
  pageSize: number = 20;
  totalPages: number = 0;
  filteredYear: string = "";
  filteredMonth: string = "";

  readonly addOnBlur = true;
  readonly separatorKeysCodes = [ENTER, COMMA] as const;
  readonly labels = signal<Label[]>([]);
  readonly announcer = inject(LiveAnnouncer);

  constructor(private http: HttpClient, private dialog: MatDialog, private labelService: LabelService) {}

  ngOnInit() {
    console.log("ng on init");
    this.loadImages();
    this.loadLabels();
  }

    // Label'ları backend'den yükle
    

  applyFilters(filters: { year: string; month: string }) {
    this.filteredYear = filters.year ? filters.year : "";
    this.filteredMonth = filters.month ? filters.month : "";
    this.loadImages();
  }

  openImageDialog(imageUrl: string) {
    this.dialog.open(ImageDialogComponent, {
      data: { url: imageUrl },
      width: '80%',
      height: '80%',
    });
  }

  toggleSelection(imageId: number) {
    console.log('toggleSelected : ', imageId);
    const index = this.selectedImageIds.indexOf(imageId);
    if (index === -1) {
      this.selectedImageIds.push(imageId); // ID'yi ekle
    } else {
      this.selectedImageIds.splice(index, 1); // ID'yi çıkar
    }
  }

  performAction() {
    if(this.selectedImageIds.length == 1 ) {
      this.loadSimilars(this.selectedImageIds[0]);
    }
    console.log('Selected Image IDs:', this.selectedImageIds);
    // Seçilen ID'lerle bir işlem yapabilirsiniz
  }

  deleteSelectedImages() {
    if (this.selectedImageIds.length === 0) {
      alert('Please select at least one image to delete.');
      return;
    }

    this.http.post(`/api/Image/bulk-delete`, this.selectedImageIds)
      .subscribe(() => {
        this.loadImages();
      })
  }

  getSafeUrl(url: string): string {
    return encodeURI(url);
  }

  loadImages() {
    this.selectedImageIds = [];
    const url = `/api/Image?pageNumber=${this.pageNumber}&pageSize=${this.pageSize}&year=${this.filteredYear}&month=${this.filteredMonth}`;
    this.http.get(url).subscribe((data: any) => {      
      this.images = data.data;
      console.log(data.totalPages);
      this.totalPages = data.totalPages;
      this.images = this.images.map(image => ({
        ...image,
        originalUrl : image.url,
        thumbUrl: 'api/Image/' + encodeURI(image.thumbUrl.replace(/^D:\/resimler\\/, '')).replace('#','%23'),
        url: 'api/Image/' + encodeURI(image.url.replace(/^D:\/resimler\\/, '')).replace('#','%23')
        
      }));
    });
  }

  loadSimilars(id:number) {
    const url = `/api/Image/${id}/similar`;
    this.selectedImageIds = [];
    this.http.get(url).subscribe((data: any) => {
      this.images = data.data;
      console.log(data.totalPages);
      this.totalPages = data.totalPages;
      this.images = this.images.map(image => ({
        ...image,
        originalUrl : image.url,
        url: 'api/Image/' + encodeURI(image.url.replace(/^D:\/resimler\\/, '')).replace('#','%23')
      }));
    });
  }

  nextPage() {
    if (this.pageNumber < this.totalPages) {
      this.pageNumber++;
      this.loadImages();
    }
  }

  previousPage() {
    if (this.pageNumber > 1) {
      this.pageNumber--;
      this.loadImages();
    }
  }

  viewDetails(image: any) {
    console.log('Viewing image:', image);
  }

  deleteImage(image: any) {
    console.log('Deleting image:', image);
  }



  // // Yeni bir label ekle
  // addNewLabel(): void {
  //   if (!this.newLabel.trim()) {
  //     alert('Label name cannot be empty.');
  //     return;
  //   }

  //   this.labelService.addLabel({ name: this.newLabel }).subscribe({
  //     next: (response) => {
  //       this.labels.push(response.label);
  //       this.newLabel = ''; // Giriş alanını temizle
  //     },
  //     error: (err) => console.error('Error adding label:', err)
  //   });
  // }

  // // Seçilen label'ı listeye ekle veya kaldır
  // toggleLabelSelection(label: string): void {
  //   const index = this.selectedLabels.indexOf(label);
  //   if (index > -1) {
  //     this.selectedLabels.splice(index, 1);
  //   } else {
  //     this.selectedLabels.push(label);
  //   }
  // }

  loadLabels(): void {
    this.labelService.getLabels().subscribe({
      next: (labels:Label[]) => {
        this.labels.set(labels);
        console.log(this.labels);
      },
      error: (err: any) => console.error('Error fetching labels:', err)
    });
  }

  add(event: MatChipInputEvent): void {
    const value = (event.value || '').trim();
    if (value) {
      this.labelService.addLabel({ name: value }).subscribe({
        next: (response:any) => {
          this.labels.update(labels => [...labels, response.label]);
        },
        error: (err: any) => console.error('Error adding label:', err)
      });    
    }
    // Clear the input value
    event.chipInput!.clear();
  }

  remove(label: Label): void {
    this.labels.update(labels => {
      const index = labels.indexOf(label);
      if (index < 0) {
        return labels;
      }

      labels.splice(index, 1);
      this.announcer.announce(`Removed ${label.name}`);
      return [...labels];
    });
  }

  edit(label: Label, event: MatChipEditedEvent) {
    const value = event.value.trim();

    // Remove fruit if it no longer has a name
    if (!value) {
      this.remove(label);
      return;
    }

    // Edit existing fruit
    this.labels.update(labels => {
      const index = labels.indexOf(label);
      if (index >= 0) {
        labels[index].name = value;
        return [...labels];
      }
      return labels;
    });
  }
}
