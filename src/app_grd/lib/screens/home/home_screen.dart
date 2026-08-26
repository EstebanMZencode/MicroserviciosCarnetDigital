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

  Future<void> _cerrarSesion() async {
    await _storage.clearSession();
    if (!mounted) return;
    Navigator.of(context).pushReplacement(
      MaterialPageRoute(builder: (_) => const LoginScreen()),
    );
  }

  void _irAScanner() {
    Navigator.of(context).push(
      MaterialPageRoute(builder: (_) => const ScannerScreen()),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Mis datos'),
        centerTitle: true,
        actions: [
          IconButton(
            onPressed: _cerrarSesion,
            tooltip: 'Cerrar sesión',
            icon: const Icon(Icons.logout),
          ),
        ],
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
      floatingActionButton: FutureBuilder<GuardaPerfil>(
        future: _perfilFuture,
        builder: (context, snapshot) {
          final tieneFoto = snapshot.connectionState == ConnectionState.done &&
              snapshot.hasData &&
              snapshot.data!.fotoBase64 != null &&
              snapshot.data!.fotoBase64!.isNotEmpty;

          return FloatingActionButton(
            onPressed: tieneFoto ? _irAScanner : null,
            tooltip: 'Escanear carnet',
            backgroundColor: tieneFoto ? null : Colors.grey,
            child: const Icon(Icons.camera_alt),
          );
        },
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
    final sinFoto = perfil.fotoBase64 == null || perfil.fotoBase64!.isEmpty;

    const acento = Color(0xFF3F51B5);
    const azulMedio = Color(0xFF1A237E);

    return Center(
      child: SingleChildScrollView(
        padding: const EdgeInsets.all(24),
        child: Container(
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(16),
            boxShadow: [
              BoxShadow(
                color: Colors.black.withValues(alpha: 0.07),
                blurRadius: 12,
                offset: const Offset(0, 4),
              ),
            ],
          ),
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              CircleAvatar(
                radius: 64,
                backgroundColor: const Color(0xFFE2E8F0),
                backgroundImage: fotoProvider,
                child: fotoProvider == null
                    ? const Icon(Icons.person,
                        size: 64, color: Color(0xFFA0AEC0))
                    : null,
              ),
              if (sinFoto) ...[
                const SizedBox(height: 12),
                Container(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
                  decoration: BoxDecoration(
                    color: const Color(0xFFFFF3CD),
                    borderRadius: BorderRadius.circular(8),
                    border: Border.all(color: const Color(0xFFFFE69C)),
                  ),
                  child: const Text(
                    'No se validará el uso de esta aplicación hasta que haya '
                    'registrado su fotografía.',
                    textAlign: TextAlign.center,
                    style: TextStyle(
                      fontSize: 13,
                      color: Color(0xFF856404),
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ),
              ],
              const SizedBox(height: 24),
              Text(
                perfil.nombreCompleto,
                textAlign: TextAlign.center,
                style: const TextStyle(
                  fontSize: 20,
                  fontWeight: FontWeight.w700,
                  color: azulMedio,
                ),
              ),
              const SizedBox(height: 20),
              _buildFila(Icons.badge_outlined, 'Identificación',
                  perfil.identificacion, acento),
              const Divider(height: 24),
              _buildFila(Icons.person_outline, 'Tipo de usuario',
                  perfil.tipoUsuario, acento),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildFila(
      IconData icono, String etiqueta, String valor, Color acento) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(icono, color: acento, size: 20),
        const SizedBox(width: 12),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                etiqueta,
                style: const TextStyle(
                  fontSize: 12,
                  color: Color(0xFF6B7280),
                  fontWeight: FontWeight.w500,
                ),
              ),
              const SizedBox(height: 2),
              Text(
                valor,
                style: const TextStyle(
                  fontSize: 15,
                  color: Color(0xFF111827),
                  fontWeight: FontWeight.w600,
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}
