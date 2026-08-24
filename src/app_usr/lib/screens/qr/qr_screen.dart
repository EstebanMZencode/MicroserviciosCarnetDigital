import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:app_usr/services/qr_service.dart';
import 'package:app_usr/services/secure_storage_service.dart';

class QrScreen extends StatefulWidget {
  const QrScreen({super.key});

  @override
  State<QrScreen> createState() => _QrScreenState();
}

class _QrScreenState extends State<QrScreen> {
  late final QRService _qrService;

  bool _cargando = true;
  String? _error;
  String? _qrBase64;

  @override
  void initState() {
    super.initState();
    _qrService = QRService(SecureStorageService());
    _cargarQr();
  }

  Future<void> _cargarQr() async {
    setState(() {
      _cargando = true;
      _error = null;
    });

    try {
      final data = await _qrService.obtenerQr();
      setState(() {
        _qrBase64 = data['qrBase64'] as String;
        _cargando = false;
      });
    } catch (e) {
      setState(() {
        _error = e.toString().replaceAll('Exception: ', '');
        _cargando = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Mi código QR'),
        centerTitle: true,
        backgroundColor: const Color(0xFF1E4CFF),
        foregroundColor: Colors.white,
      ),
      body: Center(child: _construirContenido()),
    );
  }

  Widget _construirContenido() {
    if (_cargando) {
      return const Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          CircularProgressIndicator(),
          SizedBox(height: 16),
          Text('Generando tu código QR...'),
        ],
      );
    }

    if (_error != null) {
      return Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.error_outline, color: Colors.red, size: 48),
            const SizedBox(height: 16),
            Text(
              _error!,
              textAlign: TextAlign.center,
              style: const TextStyle(fontSize: 16),
            ),
            const SizedBox(height: 24),
            ElevatedButton.icon(
              onPressed: _cargarQr,
              icon: const Icon(Icons.refresh),
              label: const Text('Reintentar'),
            ),
          ],
        ),
      );
    }

    // Mostrar el QR
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: [
        const Text(
          'Presenta este código al personal de seguridad',
          textAlign: TextAlign.center,
          style: TextStyle(fontSize: 16),
        ),
        const SizedBox(height: 24),
        Container(
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(12),
            boxShadow: [
              BoxShadow(
                color: Colors.black.withValues(alpha: 0.1),
                blurRadius: 10,
                spreadRadius: 2,
              ),
            ],
          ),
          child: Image.memory(
            base64Decode(_qrBase64!),
            width: 250,
            height: 250,
          ),
        ),
      ],
    );
  }
}