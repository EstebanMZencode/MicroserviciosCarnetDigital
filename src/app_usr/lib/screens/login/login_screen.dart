import 'package:flutter/material.dart';
import 'package:app_usr/services/i_auth_service.dart';
import 'package:app_usr/services/i_home_service.dart';
import 'package:app_usr/screens/home/home_screen.dart';

/// Pantalla de inicio de sesión.
/// Recibe las dependencias por constructor siguiendo el principio de inversión
/// de dependencias: trabaja contra las interfaces, no las implementaciones.
class LoginScreen extends StatefulWidget {
  final IAuthService authService;
  final IHomeService homeService;

  const LoginScreen({
    super.key,
    required this.authService,
    required this.homeService,
  });

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _emailCtrl    = TextEditingController();
  final _passCtrl     = TextEditingController();
  bool  _cargando     = false;
  bool  _verPassword  = false;

  // Paleta idéntica al gradiente del SitioAdministrativo
  static const Color _azulOscuro  = Color(0xFF0D1B6E);
  static const Color _azulMedio   = Color(0xFF1A237E);
  static const Color _azulClaro   = Color(0xFF3949AB);
  static const Color _acento      = Color(0xFF3F51B5);

  @override
  void dispose() {
    _emailCtrl.dispose();
    _passCtrl.dispose();
    super.dispose();
  }

  // ── Lógica ───────────────────────────────────────────────────────────────

  Future<void> _ingresar() async {
    final email    = _emailCtrl.text.trim();
    final password = _passCtrl.text;

    if (email.isEmpty || password.isEmpty) {
      _mostrarError('Por favor complete todos los campos.');
      return;
    }

    setState(() => _cargando = true);

    try {
      await widget.authService.login(email, password);
      if (!mounted) return;
      // Navegar a Home reemplazando la pila para que no se pueda volver
      Navigator.pushReplacement(
        context,
        MaterialPageRoute(
          builder: (_) => HomeScreen(
            authService: widget.authService,
            homeService: widget.homeService,
          ),
        ),
      );
    } catch (e) {
      if (!mounted) return;
      _mostrarError('Usuario y/o contraseña incorrectos.');
    } finally {
      if (mounted) setState(() => _cargando = false);
    }
  }

  void _mostrarError(String mensaje) {
    showDialog(
      context: context,
      builder: (ctx) => Dialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  const Text(
                    'Error',
                    style: TextStyle(fontSize: 18, fontWeight: FontWeight.w700),
                  ),
                  IconButton(
                    icon: const Icon(Icons.close),
                    onPressed: () => Navigator.pop(ctx),
                  ),
                ],
              ),
              const SizedBox(height: 8),
              Text(
                mensaje,
                style: const TextStyle(fontSize: 14, color: Color(0xFF374151)),
              ),
              const SizedBox(height: 20),
              Align(
                alignment: Alignment.centerRight,
                child: ElevatedButton(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: _acento,
                    foregroundColor: Colors.white,
                    shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(8)),
                  ),
                  onPressed: () => Navigator.pop(ctx),
                  child: const Text('Cerrar'),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  // ── UI ────────────────────────────────────────────────────────────────────

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Container(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
            colors: [_azulOscuro, _azulMedio, _azulClaro, Color(0xFF283593)],
            stops: [0.0, 0.3, 0.7, 1.0],
          ),
        ),
        child: SafeArea(
          child: Center(
            child: SingleChildScrollView(
              padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 32),
              child: Container(
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(20),
                  boxShadow: [
                    BoxShadow(
                      color: Colors.black.withValues(alpha: 0.28),
                      blurRadius: 64,
                      offset: const Offset(0, 24),
                    ),
                  ],
                ),
                padding: const EdgeInsets.symmetric(
                    horizontal: 32, vertical: 40),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    _buildLogo(),
                    const SizedBox(height: 32),
                    _buildCampo(
                      controller: _emailCtrl,
                      label: 'Email institucional',
                      hint: 'usuario@cuc.cr',
                      icono: Icons.email_outlined,
                      teclado: TextInputType.emailAddress,
                    ),
                    const SizedBox(height: 16),
                    _buildCampoPassword(),
                    const SizedBox(height: 28),
                    _buildBotonIngresar(),
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildLogo() {
    return Column(
      children: [
        Container(
          width: 64,
          height: 64,
          decoration: BoxDecoration(
            color: _azulMedio,
            borderRadius: BorderRadius.circular(16),
            boxShadow: [
              BoxShadow(
                color: _azulMedio.withValues(alpha: 0.35),
                blurRadius: 14,
                offset: const Offset(0, 4),
              ),
            ],
          ),
          child: const Center(
            child: _CarnetIcon(),
          ),
        ),
        const SizedBox(height: 16),
        const Text(
          'Carnet Digital',
          style: TextStyle(
            fontSize: 22,
            fontWeight: FontWeight.w700,
            color: _azulMedio,
          ),
        ),
        const SizedBox(height: 4),
        const Text(
          'Colegio Universitario de Cartago',
          style: TextStyle(fontSize: 13, color: Color(0xFF6B7280)),
        ),
      ],
    );
  }

  Widget _buildCampo({
    required TextEditingController controller,
    required String label,
    required String hint,
    required IconData icono,
    TextInputType teclado = TextInputType.text,
  }) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          label,
          style: const TextStyle(
              fontSize: 13, fontWeight: FontWeight.w500,
              color: Color(0xFF374151)),
        ),
        const SizedBox(height: 6),
        TextField(
          controller:  controller,
          keyboardType: teclado,
          decoration: InputDecoration(
            hintText: hint,
            hintStyle: const TextStyle(color: Color(0xFF9CA3AF)),
            prefixIcon: Icon(icono, color: const Color(0xFF9CA3AF), size: 20),
            contentPadding: const EdgeInsets.symmetric(
                horizontal: 12, vertical: 12),
            border: OutlineInputBorder(
              borderRadius: BorderRadius.circular(10),
              borderSide: const BorderSide(color: Color(0xFFE5E7EB)),
            ),
            enabledBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(10),
              borderSide: const BorderSide(color: Color(0xFFE5E7EB)),
            ),
            focusedBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(10),
              borderSide: const BorderSide(color: _acento, width: 1.5),
            ),
            filled: true,
            fillColor: Colors.white,
          ),
        ),
      ],
    );
  }

  Widget _buildCampoPassword() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Text(
          'Contraseña',
          style: TextStyle(
              fontSize: 13, fontWeight: FontWeight.w500,
              color: Color(0xFF374151)),
        ),
        const SizedBox(height: 6),
        TextField(
          controller:  _passCtrl,
          obscureText: !_verPassword,
          decoration: InputDecoration(
            hintText: '••••••••',
            hintStyle: const TextStyle(color: Color(0xFF9CA3AF)),
            prefixIcon: const Icon(Icons.lock_outline,
                color: Color(0xFF9CA3AF), size: 20),
            suffixIcon: IconButton(
              icon: Icon(
                _verPassword ? Icons.visibility_off : Icons.visibility,
                color: const Color(0xFF9CA3AF),
                size: 20,
              ),
              onPressed: () => setState(() => _verPassword = !_verPassword),
            ),
            contentPadding: const EdgeInsets.symmetric(
                horizontal: 12, vertical: 12),
            border: OutlineInputBorder(
              borderRadius: BorderRadius.circular(10),
              borderSide: const BorderSide(color: Color(0xFFE5E7EB)),
            ),
            enabledBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(10),
              borderSide: const BorderSide(color: Color(0xFFE5E7EB)),
            ),
            focusedBorder: OutlineInputBorder(
              borderRadius: BorderRadius.circular(10),
              borderSide: const BorderSide(color: _acento, width: 1.5),
            ),
            filled: true,
            fillColor: Colors.white,
          ),
          onSubmitted: (_) => _ingresar(),
        ),
      ],
    );
  }

  Widget _buildBotonIngresar() {
    return SizedBox(
      width: double.infinity,
      child: ElevatedButton(
        style: ElevatedButton.styleFrom(
          backgroundColor: _acento,
          foregroundColor: Colors.white,
          padding: const EdgeInsets.symmetric(vertical: 14),
          shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(10)),
          elevation: 0,
        ),
        onPressed: _cargando ? null : _ingresar,
        child: _cargando
            ? const SizedBox(
                width: 20,
                height: 20,
                child: CircularProgressIndicator(
                    strokeWidth: 2, color: Colors.white),
              )
            : const Text(
                'Ingresar',
                style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600),
              ),
      ),
    );
  }
}

// ── Icono del carnet digital dibujado con CustomPainter ──────────────────────
// Reproduce el SVG del SitioAdministrativo: rect con rx, círculo, líneas
class _CarnetIcon extends StatelessWidget {
  const _CarnetIcon();

  @override
  Widget build(BuildContext context) {
    return CustomPaint(
      size: const Size(28, 28),
      painter: _CarnetPainter(),
    );
  }
}

class _CarnetPainter extends CustomPainter {
  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color  = Colors.white
      ..style  = PaintingStyle.stroke
      ..strokeWidth = 1.8
      ..strokeCap   = StrokeCap.round;

    final double sx = size.width  / 24;
    final double sy = size.height / 24;

    // Rect exterior con esquinas redondeadas (rx=2)
    final rrect = RRect.fromLTRBR(
      3 * sx, 5 * sy, 21 * sx, 19 * sy,
      Radius.circular(2 * sx),
    );
    canvas.drawRRect(rrect, paint);

    // Círculo interior izquierdo
    canvas.drawCircle(Offset(9 * sx, 10 * sy), 2 * sx, paint);

    // Líneas de texto (derecha)
    canvas.drawLine(Offset(15 * sx, 8 * sy), Offset(17 * sx, 8 * sy), paint);
    canvas.drawLine(Offset(15 * sx, 12 * sy), Offset(17 * sx, 12 * sy), paint);
    // Línea inferior completa
    canvas.drawLine(Offset(7 * sx, 15 * sy), Offset(17 * sx, 15 * sy), paint);
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) => false;
}
