import 'package:flutter/material.dart';
import 'package:app_grd/screens/home/home_screen.dart';
import 'package:app_grd/services/auth_service.dart';
import 'package:app_grd/services/secure_storage_service.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _formKey = GlobalKey<FormState>();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  late final AuthService _authService;

  bool _cargando = false;
  String? _errorMensaje;

  @override
  void initState() {
    super.initState();
    _authService = AuthService(SecureStorageService());
  }

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _ingresar() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() {
      _cargando = true;
      _errorMensaje = null;
    });

    try {
      await _authService.login(
        _emailController.text.trim(),
        _passwordController.text,
      );

      if (!mounted) return;
      Navigator.of(context).pushReplacement(
        MaterialPageRoute(builder: (_) => const HomeScreen()),
      );
    } catch (e) {
      setState(() {
        _errorMensaje = e.toString().replaceFirst('Exception: ', '');
      });
    } finally {
      if (mounted) setState(() => _cargando = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.symmetric(horizontal: 32),
            child: Form(
              key: _formKey,
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  const Center(child: _LogoCuc()),
                  const SizedBox(height: 16),
                  const Text(
                    'Carnet Digital',
                    textAlign: TextAlign.center,
                    style: TextStyle(
                      fontSize: 22,
                      fontWeight: FontWeight.w700,
                      color: Color(0xFF1A237E),
                    ),
                  ),
                  const SizedBox(height: 10),
                  const Center(child: _BadgeGuarda()),
                  const SizedBox(height: 8),
                  const Text(
                    'Ingrese con sus credenciales de personal de seguridad',
                    textAlign: TextAlign.center,
                    style: TextStyle(fontSize: 13, color: Color(0xFF6B7280)),
                  ),
                  const SizedBox(height: 32),
                  TextFormField(
                    controller: _emailController,
                    keyboardType: TextInputType.emailAddress,
                    textInputAction: TextInputAction.next,
                    decoration: const InputDecoration(
                      labelText: 'Correo electrónico',
                      prefixIcon: Icon(Icons.email_outlined),
                    ),
                    validator: (value) {
                      if (value == null || value.trim().isEmpty) {
                        return 'Ingrese su correo electrónico.';
                      }
                      if (!value.contains('@')) {
                        return 'Ingrese un correo electrónico válido.';
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 16),
                  TextFormField(
                    controller: _passwordController,
                    obscureText: true,
                    textInputAction: TextInputAction.done,
                    onFieldSubmitted: (_) => _ingresar(),
                    decoration: const InputDecoration(
                      labelText: 'Contraseña',
                      prefixIcon: Icon(Icons.lock_outline),
                    ),
                    validator: (value) {
                      if (value == null || value.isEmpty) {
                        return 'Ingrese su contraseña.';
                      }
                      return null;
                    },
                  ),
                  if (_errorMensaje != null) ...[
                    const SizedBox(height: 16),
                    Text(
                      _errorMensaje!,
                      textAlign: TextAlign.center,
                      style: TextStyle(color: Theme.of(context).colorScheme.error),
                    ),
                  ],
                  const SizedBox(height: 24),
                  FilledButton(
                    onPressed: _cargando ? null : _ingresar,
                    child: _cargando
                        ? const SizedBox(
                            height: 20,
                            width: 20,
                            child: CircularProgressIndicator(
                              strokeWidth: 2,
                              color: Colors.white,
                            ),
                          )
                        : const Text('Ingresar'),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}

// Distintivo que deja claro que este es el acceso de guardas y no el de
// estudiantes (app_usr), reutilizando el mismo color de acento.
class _BadgeGuarda extends StatelessWidget {
  const _BadgeGuarda();

  static const Color _acento = Color(0xFF3F51B5);

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      decoration: BoxDecoration(
        color: _acento.withValues(alpha: 0.1),
        borderRadius: BorderRadius.circular(20),
      ),
      child: const Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(Icons.shield_outlined, size: 14, color: _acento),
          SizedBox(width: 6),
          Text(
            'GUARDAS',
            style: TextStyle(
              fontSize: 12,
              fontWeight: FontWeight.w700,
              color: _acento,
              letterSpacing: 0.6,
            ),
          ),
        ],
      ),
    );
  }
}

// Logo del carnet. app_usr no usa ningún asset de imagen: dibuja este mismo
// ícono con CustomPainter dentro de un cuadrado redondeado. Se replica aquí
// tal cual para mantener el mismo lenguaje visual entre ambas apps.
class _LogoCuc extends StatelessWidget {
  const _LogoCuc();

  static const Color _azulMedio = Color(0xFF1A237E);

  @override
  Widget build(BuildContext context) {
    return Container(
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
      child: const Center(child: _CarnetIcon()),
    );
  }
}

// Reproduce el ícono del carnet digital de app_usr (rect con esquinas
// redondeadas, círculo y líneas de texto), dibujado con CustomPainter.
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
      ..color = Colors.white
      ..style = PaintingStyle.stroke
      ..strokeWidth = 1.8
      ..strokeCap = StrokeCap.round;

    final double sx = size.width / 24;
    final double sy = size.height / 24;

    final rrect = RRect.fromLTRBR(
      3 * sx, 5 * sy, 21 * sx, 19 * sy,
      Radius.circular(2 * sx),
    );
    canvas.drawRRect(rrect, paint);

    canvas.drawCircle(Offset(9 * sx, 10 * sy), 2 * sx, paint);

    canvas.drawLine(Offset(15 * sx, 8 * sy), Offset(17 * sx, 8 * sy), paint);
    canvas.drawLine(Offset(15 * sx, 12 * sy), Offset(17 * sx, 12 * sy), paint);
    canvas.drawLine(Offset(7 * sx, 15 * sy), Offset(17 * sx, 15 * sy), paint);
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) => false;
}
