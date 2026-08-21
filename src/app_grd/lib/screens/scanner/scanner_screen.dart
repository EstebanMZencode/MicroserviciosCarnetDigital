import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:mobile_scanner/mobile_scanner.dart';
import 'package:audioplayers/audioplayers.dart';
import 'package:app_grd/models/usuario_qr_dto.dart';
import 'package:app_grd/services/scanner_service.dart';
import 'package:app_grd/services/secure_storage_service.dart';

class ScannerScreen extends StatefulWidget {
  const ScannerScreen({super.key});

  @override
  State<ScannerScreen> createState() => _ScannerScreenState();
}

class _ScannerScreenState extends State<ScannerScreen> {
  final MobileScannerController _cameraController = MobileScannerController();
  final AudioPlayer _audioPlayer = AudioPlayer();
  late final ScannerService _scannerService;

  bool _procesando = false;
  bool? _ultimoResultado;
  String _mensaje = '';

  @override
  void initState() {
    super.initState();
    _scannerService = ScannerService(SecureStorageService());
  }

  @override
  void dispose() {
    _cameraController.dispose();
    _audioPlayer.dispose();
    super.dispose();
  }

  Future<void> _procesarQr(String contenido) async {
    if (_procesando) return;

    setState(() => _procesando = true);

    try {
      final json = jsonDecode(contenido) as Map<String, dynamic>;
      final dto = UsuarioQrDto.fromJson(json);
      final resultado = await _scannerService.validarQr(dto);

      await _reproducirSonido(resultado.valido);

      setState(() {
        _ultimoResultado = resultado.valido;
        _mensaje = resultado.mensaje;
      });
    } catch (e) {
      await _reproducirSonido(false);
      setState(() {
        _ultimoResultado = false;
        _mensaje = 'QR inválido o no reconocido.';
      });
    } finally {
      await Future.delayed(const Duration(seconds: 2));
      setState(() {
        _procesando = false;
        _ultimoResultado = null;
        _mensaje = '';
      });
    }
  }

  Future<void> _reproducirSonido(bool valido) async {
    await _audioPlayer.play(
      AssetSource(valido ? 'sounds/exito.mp3' : 'sounds/error.mp3'),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Escanear carnet'),
        centerTitle: true,
      ),
      body: Stack(
        children: [
          MobileScanner(
            controller: _cameraController,
            onDetect: (capture) {
              final barcode = capture.barcodes.firstOrNull;
              if (barcode?.rawValue != null) {
                _procesarQr(barcode!.rawValue!);
              }
            },
          ),
          if (_ultimoResultado != null)
            Container(
              color: _ultimoResultado!
                  ? Colors.green.withOpacity(0.7)
                  : Colors.red.withOpacity(0.7),
              child: Center(
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(
                      _ultimoResultado!
                          ? Icons.check_circle
                          : Icons.cancel,
                      color: Colors.white,
                      size: 100,
                    ),
                    const SizedBox(height: 16),
                    Text(
                      _mensaje,
                      style: const TextStyle(
                        color: Colors.white,
                        fontSize: 20,
                        fontWeight: FontWeight.bold,
                      ),
                      textAlign: TextAlign.center,
                    ),
                  ],
                ),
              ),
            ),
          if (_procesando && _ultimoResultado == null)
            const Center(child: CircularProgressIndicator()),
        ],
      ),
    );
  }
}