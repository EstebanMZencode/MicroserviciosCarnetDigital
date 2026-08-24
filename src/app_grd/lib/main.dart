import 'package:flutter/material.dart';
import 'package:app_grd/screens/login/login_screen.dart';

void main() {
  runApp(const AppGrd());
}

class AppGrd extends StatelessWidget {
  const AppGrd({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Carnet Digital - Guardas',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: Colors.indigo),
        useMaterial3: true,
      ),
      home: const LoginScreen(),
    );
  }
}