-- VidaApp - base mínima
CREATE TABLE IF NOT EXISTS schema_version_probe (
  id BIGINT PRIMARY KEY AUTO_INCREMENT,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

INSERT INTO schema_version_probe () VALUES ();
