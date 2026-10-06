CREATE DATABASE IF NOT EXISTS uno
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE uno;

CREATE TABLE IF NOT EXISTS jugadores (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL
);

CREATE TABLE IF NOT EXISTS partidas (
    id INT AUTO_INCREMENT PRIMARY KEY,
    fecha_inicio DATETIME NOT NULL,
    fecha_fin DATETIME NULL,
    ganador_id INT NULL,
    FOREIGN KEY (ganador_id) REFERENCES jugadores(id)
);

CREATE TABLE IF NOT EXISTS partida_jugador (
    partida_id INT NOT NULL,
    jugador_id INT NOT NULL,
    resultado ENUM('ganada', 'perdida') NULL,
    PRIMARY KEY (partida_id, jugador_id),
    FOREIGN KEY (partida_id) REFERENCES partidas(id),
    FOREIGN KEY (jugador_id) REFERENCES jugadores(id)
);

CREATE TABLE IF NOT EXISTS movimientos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    partida_id INT NOT NULL,
    jugador_id INT NOT NULL,
    turno INT NOT NULL,
    accion VARCHAR(30) NOT NULL,
    carta VARCHAR(30) NULL,
    color_elegido VARCHAR(10) NULL,
    fecha DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (partida_id) REFERENCES partidas(id),
    FOREIGN KEY (jugador_id) REFERENCES jugadores(id)
);

INSERT IGNORE INTO jugadores (id, nombre) VALUES
    (1, 'Paul'),
    (2, 'Rosa'),
    (3, 'Dalton');