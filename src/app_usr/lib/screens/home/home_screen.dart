import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:app_usr/models/fotografia_model.dart';
import 'package:app_usr/models/usuario_model.dart';
import 'package:app_usr/screens/qr/qr_screen.dart';
import 'package:app_usr/services/i_auth_service.dart';
import 'package:app_usr/services/i_home_service.dart';
import 'package:app_usr/screens/login/login_screen.dart';

/// Pantalla principal tras el login exitoso.
/// Muestra foto (o avatar), datos del usuario y permite navegar al QR
/// deslizando de izquierda a derecha — solo si hay fotografía registrada.
class HomeScreen extends StatefulWidget {
  final IAuthService authService;
  final IHomeService homeService;

  const HomeScreen({
    super.key,
    required this.authService,
    required this.homeService,
  });

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  static const Color _acento    = Color(0xFF3F51B5);
  static const Color _azulOscuro = Color(0xFF1A237E);

  UsuarioModel?    _usuario;
  FotografiaModel? _fotografia;
  bool  _cargando = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _cargarDatos();
  }

  // ── Carga de datos ────────────────────────────────────────────────────────

  Future<void> _cargarDatos() async {
    setState(() { _cargando = true; _error = null; });
    try {
      // Ambas llamadas en paralelo para reducir latencia
      final resultados = await Future.wait([
        widget.homeService.getUsuario(),
        widget.homeService.getFotografia(),
      ]);
      if (!mounted) return;
      setState(() {
        _usuario     = resultados[0] as UsuarioModel;
        _fotografia  = resultados[1] as FotografiaModel;
        _cargando    = false;
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _error    = e.toString().replaceAll('Exception: ', '');
        _cargando = false;
      });
    }
  }

  // ── Cerrar sesión ─────────────────────────────────────────────────────────

  Future<void> _cerrarSesion() async {
    await widget.authService.cerrarSesion();
    if (!mounted) return;
    Navigator.pushAndRemoveUntil(
      context,
      MaterialPageRoute(
        builder: (_) => LoginScreen(
          authService: widget.authService,
          homeService: widget.homeService,
        ),
      ),
      (_) => false,
    );
  }

  // ── UI ────────────────────────────────────────────────────────────────────

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF3F4F6),
      appBar: AppBar(
        backgroundColor: _azulOscuro,
        foregroundColor: Colors.white,
        title: const Text(
          'Carnet Digital',
          style: TextStyle(fontWeight: FontWeight.w600),
        ),
        centerTitle: true,
        automaticallyImplyLeading: false,
        actions: [
          // Botón de cierre de sesión en la esquina superior derecha
          IconButton(
            tooltip: 'Cerrar sesión',
            icon: const Icon(Icons.logout),
            onPressed: _cerrarSesion,
          ),
        ],
      ),
      body: _construirCuerpo(),
    );
  }

  Widget _construirCuerpo() {
    if (_cargando) {
      return const Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            CircularProgressIndicator(),
            SizedBox(height: 16),
            Text('Cargando datos...'),
          ],
        ),
      );
    }

    if (_error != null) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Icon(Icons.error_outline, color: Colors.red, size: 48),
              const SizedBox(height: 16),
              Text(_error!, textAlign: TextAlign.center),
              const SizedBox(height: 24),
              ElevatedButton.icon(
                onPressed: _cargarDatos,
                icon: const Icon(Icons.refresh),
                label: const Text('Reintentar'),
              ),
            ],
          ),
        ),
      );
    }

    final tieneFoto = _fotografia?.tieneFoto ?? false;

    // Dismissible envuelve toda la pantalla.
    // confirmDismiss bloquea el gesto si no hay foto sin mostrar animación de rechazo.
    return Dismissible(
      key: const ValueKey('home'),
      direction: DismissDirection.startToEnd,
      confirmDismiss: (_) async {
        if (!tieneFoto) return false; // sin foto: bloquear silenciosamente
        if (!mounted) return false;
        Navigator.push(
          context,
          MaterialPageRoute(builder: (_) => QrScreen(authService: widget.authService)),
        );
        return false; // no descartar el widget, solo navegar
      },
      // Indicador visual de deslizamiento hacia el QR
      background: Container(
        color: _acento.withValues(alpha: 0.15),
        alignment: Alignment.centerLeft,
        padding: const EdgeInsets.only(left: 24),
        child: const Icon(Icons.qr_code_2, color: _acento, size: 40),
      ),
      child: SingleChildScrollView(
        padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 24),
        child: Column(
          children: [
            _buildTarjetaFoto(tieneFoto),
            const SizedBox(height: 20),
            _buildTarjetaDatos(),
            const SizedBox(height: 20),
            if (!tieneFoto)
              _buildAvisoSinFoto()
            /*if (tieneFoto)
              _buildIndicadorQr()
            else
              _buildAvisoSinFoto(),*/
          ],
        ),
      ),
    );
  }

  // ── Sección de foto ───────────────────────────────────────────────────────

  Widget _buildTarjetaFoto(bool tieneFoto) {
    return Container(
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
      child: Center(
        child: tieneFoto
            ? ClipRRect(
                borderRadius: BorderRadius.circular(12),
                child: Image.memory(
                  base64Decode(_fotografia!.fotoBase64!),
                  width: 180,
                  height: 135, // proporción 4:3
                  fit: BoxFit.cover,
                ),
              )
            : _buildAvatar(),
      ),
    );
  }

  Widget _buildAvatar() {
    return Column(
      children: [
        Container(
          width: 120,
          height: 120,
          decoration: BoxDecoration(
            color: const Color(0xFFE2E8F0),
            borderRadius: BorderRadius.circular(60),
          ),
          child: const Icon(
            Icons.person,
            size: 72,
            color: Color(0xFFA0AEC0),
          ),
        ),
      ],
    );
  }

  // ── Datos del usuario ─────────────────────────────────────────────────────

  Widget _buildTarjetaDatos() {
    final u = _usuario!;
    return Container(
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
      padding: const EdgeInsets.all(20),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Nombre completo
          _buildFila('Nombre completo', u.nombreCompleto),
          const Divider(height: 24),
          // Identificación
          _buildFila('Identificación', u.identificacion),
          const Divider(height: 24),
          // Tipo de usuario
          _buildFila('Tipo de usuario', u.tipoUsuario),
          // Carreras (si aplica)
          if (u.carreras.isNotEmpty) ...[
            const Divider(height: 24),
            _buildFilaLista('Carreras', u.carreras),
          ],
          // Áreas de trabajo (si aplica)
          if (u.areas.isNotEmpty) ...[
            const Divider(height: 24),
            _buildFilaLista('Áreas de trabajo', u.areas),
          ],
        ],
      ),
    );
  }

  Widget _buildFila(String etiqueta, String valor) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(Icons.info_outline, color: Colors.white, size: 20),
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

  Widget _buildFilaLista(String etiqueta, List<String> items) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(Icons.info_outline, color: Colors.white, size: 20),
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
              const SizedBox(height: 4),
              ...items.map(
                (item) => Padding(
                  padding: const EdgeInsets.only(bottom: 2),
                  child: Text(
                    '• $item',
                    style: const TextStyle(
                      fontSize: 15,
                      color: Color(0xFF111827),
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }

  // ── Indicadores de QR y aviso sin foto ───────────────────────────────────

  /*Widget _buildIndicadorQr() {
    return Container(
      decoration: BoxDecoration(
        color: _acento.withValues(alpha: 0.08),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: _acento.withValues(alpha: 0.3)),
      ),
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      child: const Row(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(Icons.swipe_right_alt, color: _acento),
          SizedBox(width: 8),
          Text(
            'Desliza hacia la derecha para ver tu QR',
            style: TextStyle(color: _acento, fontWeight: FontWeight.w500),
          ),
        ],
      ),
    );
  }*/

  Widget _buildAvisoSinFoto() {
    return Container(
      decoration: BoxDecoration(
        color: const Color(0xFFFEF3C7),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: const Color(0xFFF59E0B).withValues(alpha: 0.5)),
      ),
      padding: const EdgeInsets.all(16),
      child: const Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Icon(Icons.warning_amber_rounded,
              color: Color(0xFFD97706), size: 22),
          SizedBox(width: 10),
          Expanded(
            child: Text(
              'No se validará el uso de esta aplicación hasta que haya registrado su fotografía.',
              style: TextStyle(
                color: Color(0xFF92400E),
                fontSize: 13,
                fontWeight: FontWeight.w500,
              ),
            ),
          ),
        ],
      ),
    );
  }
}
