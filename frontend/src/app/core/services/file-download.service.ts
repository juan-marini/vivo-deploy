import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class FileDownloadService {

  constructor(private http: HttpClient) {}

  downloadFileDirectly(fileName: string): void {
    console.log('🔄 Forçando download real do arquivo:', fileName);

    // Usar a API de download que força o download
    const downloadUrl = `http://localhost:5000/api/file/download/${fileName}`;
    console.log('📥 URL da API de download:', downloadUrl);

    // Método que força download via HttpClient + Blob
    this.downloadViaBlob(downloadUrl, fileName);
  }

  private downloadViaBlob(url: string, fileName: string): void {
    console.log('💾 Baixando via blob para forçar download...');

    this.http.get(url, {
      responseType: 'blob',
      observe: 'response'
    }).subscribe({
      next: (response) => {
        console.log('✅ Arquivo baixado como blob:', response.status);

        if (response.body) {
          this.saveFileToComputer(response.body, fileName);
        } else {
          console.error('❌ Blob vazio recebido');
          this.fallbackDirectDownload(url, fileName);
        }
      },
      error: (error) => {
        console.error('❌ Erro no download via blob:', error);
        this.fallbackDirectDownload(url, fileName);
      }
    });
  }

  private saveFileToComputer(blob: Blob, fileName: string): void {
    console.log('💾 Salvando arquivo no computador:', fileName);

    try {
      // Criar URL do blob
      const blobUrl = window.URL.createObjectURL(blob);

      // Criar link temporário
      const link = document.createElement('a');
      link.href = blobUrl;
      link.download = fileName; // Força o download
      link.style.display = 'none';

      // Adicionar ao DOM e clicar
      document.body.appendChild(link);
      link.click();

      // Limpar recursos
      setTimeout(() => {
        document.body.removeChild(link);
        window.URL.revokeObjectURL(blobUrl);
        console.log('✅ Download concluído e recursos limpos');
      }, 100);

    } catch (error) {
      console.error('❌ Erro ao salvar arquivo:', error);
      this.fallbackDirectDownload(`http://localhost:5000/api/file/download/${fileName}`, fileName);
    }
  }

  private fallbackDirectDownload(url: string, fileName: string): void {
    console.log('🔄 Usando fallback: window.location.href');

    try {
      // Forçar download direto
      window.location.href = url;
      console.log('✅ Fallback executado');
    } catch (error) {
      console.error('❌ Erro no fallback:', error);
      alert(`Erro no download. Tente acessar diretamente: ${url}`);
    }
  }
}