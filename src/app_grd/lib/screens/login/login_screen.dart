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
                  const _LogoCuc(),
                  const SizedBox(height: 12),
                  Text(
                    'Carnet Digital - Guardas',
                    textAlign: TextAlign.center,
                    style: Theme.of(context).textTheme.titleMedium,
                  ),
                  const SizedBox(height: 32),
                  TextFormField(
                    controller: _emailController,
                    keyboardType: TextInputType.emailAddress,
                    textInputAction: TextInputAction.next,
                    decoration: const InputDecoration(
                      labelText: 'Correo electrónico',
                      prefixIcon: Icon(Icons.email_outlined),
                      border: OutlineInputBorder(),
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
                      border: OutlineInputBorder(),
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

// Logo del CUC. Placeholder dibujado en código: no hay ningún asset de
// imagen del logo en el repositorio todavía. Reemplazar por Image.asset(...)
// cuando se disponga del archivo real.
class _LogoCuc extends StatelessWidget {
  const _LogoCuc();

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;
    return CircleAvatar(
      radius: 48,
      backgroundColor: colorScheme.primary,
      child: const Text(
        'CUC',
        style: TextStyle(
          color: Colors.white,
          fontSize: 22,
          fontWeight: FontWeight.bold,
          letterSpacing: 1,
        ),
      ),
    );
  }
}
