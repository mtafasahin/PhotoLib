import { Component, Inject, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-image-dialog',
  standalone: true,
  template: `
    <div class="dialog-container">
      <img #dialogImage [src]="data.url" alt="Preview Image" class="dialog-image" />
    </div>
  `,
  styles: [
    `
      .dialog-container {
        display: flex;
        justify-content: center;
        align-items: center;
        overflow: hidden;
      }
      .dialog-image {
        max-width: 100%;
        max-height: 100%;
      }
    `,
  ],
})
export class ImageDialogComponent implements AfterViewInit {
  @ViewChild('dialogImage', { static: false }) dialogImage!: ElementRef<HTMLImageElement>;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: { url: string },
    private dialogRef: MatDialogRef<ImageDialogComponent>
  ) {}

  ngAfterViewInit() {
    // Resmin boyutlarını al ve dialog boyutunu ayarla
    const imageElement = this.dialogImage.nativeElement;
    imageElement.onload = () => {
      const width = imageElement.naturalWidth;
      const height = imageElement.naturalHeight;

      // Dialog boyutlarını ayarla
      this.dialogRef.updateSize(`${width}px`, `${height}px`);
    };
  }
}
