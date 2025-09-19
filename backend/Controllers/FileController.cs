using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        [HttpGet("download/{fileName}")]
        public IActionResult DownloadFile(string fileName)
        {
            try
            {
                Console.WriteLine($"📥 Download solicitado: {fileName}");

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", fileName);
                Console.WriteLine($"📂 Caminho completo: {filePath}");

                if (!System.IO.File.Exists(filePath))
                {
                    Console.WriteLine($"❌ Arquivo não encontrado: {filePath}");
                    return NotFound($"Arquivo {fileName} não encontrado");
                }

                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                Console.WriteLine($"✅ Arquivo carregado: {fileBytes.Length} bytes");

                // Forçar download com Content-Disposition: attachment
                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro no download: {ex.Message}");
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }
}