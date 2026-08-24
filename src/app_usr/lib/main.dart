import 'package:flutter/material.dart';
import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:app_usr/screens/login/login_screen.dart';
import 'package:app_usr/services/auth_service.dart';
import 'package:app_usr/services/home_service.dart';
import 'package:app_usr/services/secure_storage_service.dart';

Future<void> main() async {
  // Necesario antes de cualquier llamada asíncrona en main()
  WidgetsFlutterBinding.ensureInitialized();

  // Cargar el .env antes de iniciar la app para que dotenv.env esté disponible
  await dotenv.load(fileName: '.env');

  // Crear las dependencias una sola vez y pasarlas hacia abajo (inyección manual)
  final storage  = SecureStorageService();
  final authSvc  = AuthService(storage);
  final homeSvc  = HomeService(storage);

  runApp(AppUsuarios(authService: authSvc, homeService: homeSvc));
}

class AppUsuarios extends StatelessWidget {
  final AuthService authService;
  final HomeService homeService;

  const AppUsuarios({
    super.key,
    required this.authService,
    required this.homeService,
  });

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Carnet Digital CUC',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: const Color(0xFF3F51B5)),
        fontFamily: 'Roboto',
        useMaterial3: true,
      ),
      // La app siempre arranca en Login.
      // Si la sesión ya estaba activa se podría verificar aquí y redirigir
      // a Home, pero por simplicidad y seguridad siempre se pide credenciales.
      home: LoginScreen(
        authService: authService,
        homeService: homeService,
      ),
    );
  }
}
