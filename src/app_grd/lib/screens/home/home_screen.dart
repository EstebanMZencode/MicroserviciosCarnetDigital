import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:app_grd/models/guarda_perfil.dart';
import 'package:app_grd/screens/login/login_screen.dart';
import 'package:app_grd/screens/scanner/scanner_screen.dart';
import 'package:app_grd/services/home_service.dart';
import 'package:app_grd/services/secure_storage_service.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  final SecureStorageService _storage = SecureStorageService();
  late final HomeService _homeService;
  late final Future<GuardaPerfil> _perfilFuture;

  @override
  void initState() {
    super.initState();
    _homeService = HomeService(_storage);
    _perfilFuture = _cargarPerfil();
  }

  Future<GuardaPerfil> _cargarPerfil() async {
    final token = await _storage.getAccessToken();
    final email = await _storage.getUsuarioId();

    if (token == null || token.isEmpty || email == null || email.isEmpty) {
      throw SesionExpiradaException();
    }

    return _homeService.obtenerPerfil(email);
  }

  void _volverAlLogin() {
    _storage.clearSession();
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted) return;
      Navigator.of(context).pushReplacement(
        MaterialPageRoute(builder: (_) => const LoginScreen()),
      );
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Mis datos'),
        centerTitle: true,
      ),
      body: FutureBuilder<GuardaPerfil>(
        future: _perfilFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState != ConnectionState.done) {
            return const Center(child: CircularProgressIndicator());
          }

          if (snapshot.hasError) {
            if (snapshot.error is SesionExpiradaException) {
              _volverAlLogin();
              return const SizedBox.shrink();
            }
            return Center(
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Text(
                  'No se pudo cargar la información. Intente de nuevo más tarde.',
                  textAlign: TextAlign.center,
                ),
              ),
            );
          }

          return _PerfilBody(perfil: snapshot.data!);
        },
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () {
          Navigator.of(context).push(
            MaterialPageRoute(builder: (_) => const ScannerScreen()),
          );
        },
        tooltip: 'Escanear carnet',
        child: const Icon(Icons.camera_alt),
      ),
    );
  }
}

class _PerfilBody extends StatelessWidget {
  final GuardaPerfil perfil;

  const _PerfilBody({required this.perfil});

  @override
  Widget build(BuildContext context) {
    ImageProvider? fotoProvider;
    if (perfil.fotoBase64 != null && perfil.fotoBase64!.isNotEmpty) {
      try {
        fotoProvider = MemoryImage(base64Decode(perfil.fotoBase64!));
      } catch (_) {
        fotoProvider = null;
      }
    }

    return Center(
      child: SingleChildScrollView(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            CircleAvatar(
              radius: 64,
              backgroundImage: fotoProvider,
              child: fotoProvider == null
                  ? const Icon(Icons.person, size: 64)
                  : null,
            ),
            const SizedBox(height: 24),
            Text(
              perfil.nombreCompleto,
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.headlineSmall,
            ),
            const SizedBox(height: 12),
            Text('Identificación: ${perfil.identificacion}'),
            const SizedBox(height: 4),
            Text('Tipo de usuario: ${perfil.tipoUsuario}'),
          ],
        ),
      ),
    );
  }
}
