let stream = null;
let currentFacingMode = 'environment'; // 'user' für Frontkamera, 'environment' für Rückkamera

export async function startCamera(videoElement) {
    try {
        const constraints = {
            video: {
                facingMode: currentFacingMode,
                width: { ideal: 1280 },
                height: { ideal: 720 }
            }
        };

        stream = await navigator.mediaDevices.getUserMedia(constraints);
        videoElement.srcObject = stream;
        
        return true;
    } catch (error) {
        console.error('Fehler beim Zugriff auf Kamera:', error);
        
        // Fallback: Verwende verfügbare Kamera
        try {
            stream = await navigator.mediaDevices.getUserMedia({ video: true });
            videoElement.srcObject = stream;
            return true;
        } catch (fallbackError) {
            console.error('Kein Kamerazugriff möglich:', fallbackError);
            return false;
        }
    }
}

export function stopCamera() {
    if (stream) {
        stream.getTracks().forEach(track => track.stop());
        stream = null;
    }
}

export async function switchCamera(videoElement) {
    stopCamera();
    currentFacingMode = currentFacingMode === 'user' ? 'environment' : 'user';
    await startCamera(videoElement);
}

export async function captureAndDecode(videoElement, canvasElement) {
    const canvas = canvasElement;
    const context = canvas.getContext('2d');
    
    // Canvas-Größe an Video anpassen
    canvas.width = videoElement.videoWidth;
    canvas.height = videoElement.videoHeight;
    
    // Frame vom Video auf Canvas zeichnen
    context.drawImage(videoElement, 0, 0, canvas.width, canvas.height);
    
    // Bild-Daten für Barcode-Erkennung extrahieren
    const imageData = context.getImageData(0, 0, canvas.width, canvas.height);
    
    // Einfache Barcode-Erkennung (in Produktion würde man eine Library wie QuaggaJS verwenden)
    const result = await performBarcodeDetection(imageData);
    
    return result;
}

async function performBarcodeDetection(imageData) {
    // Hier würde normalerweise eine echte Barcode-Detection-Library verwendet
    // Für Demo-Zwecke simulieren wir eine Erkennung
    
    try {
        // Verwende Browser BarcodeDetector API falls verfügbar
        if ('BarcodeDetector' in window) {
            const barcodeDetector = new BarcodeDetector({
                formats: ['code_128', 'code_39', 'qr_code', 'ean_13', 'ean_8']
            });
            
            const canvas = document.createElement('canvas');
            canvas.width = imageData.width;
            canvas.height = imageData.height;
            const ctx = canvas.getContext('2d');
            ctx.putImageData(imageData, 0, 0);
            
            const barcodes = await barcodeDetector.detect(canvas);
            
            if (barcodes.length > 0) {
                return barcodes[0].rawValue;
            }
        }
        
        // Fallback: Simulierte Erkennung für Demo
        return simulateBarcodeDetection();
        
    } catch (error) {
        console.error('Barcode-Erkennung fehlgeschlagen:', error);
        return null;
    }
}

function simulateBarcodeDetection() {
    // Simuliert eine Barcode-Erkennung für Demo-Zwecke
    // Gibt zufällig eine UUID zurück
    const year = new Date().getFullYear();
    const number = Math.floor(Math.random() * 9999).toString().padStart(4, '0');
    return `KL-${year}-${number}`;
}

// QR-Code Generator Funktion
export function generateQRCode(text, canvasElement) {
    // Einfacher QR-Code Generator (in Produktion würde man qrcode.js verwenden)
    const canvas = canvasElement;
    const ctx = canvas.getContext('2d');
    
    canvas.width = 200;
    canvas.height = 200;
    
    // Weißer Hintergrund
    ctx.fillStyle = 'white';
    ctx.fillRect(0, 0, 200, 200);
    
    // Schwarzer QR-Code (vereinfacht)
    ctx.fillStyle = 'black';
    
    // Simuliere QR-Code-Pattern
    for (let i = 0; i < 20; i++) {
        for (let j = 0; j < 20; j++) {
            if (Math.random() > 0.5) {
                ctx.fillRect(i * 10, j * 10, 10, 10);
            }
        }
    }
    
    // Text unter QR-Code
    ctx.fillStyle = 'black';
    ctx.font = '12px Arial';
    ctx.textAlign = 'center';
    ctx.fillText(text, 100, 190);
}