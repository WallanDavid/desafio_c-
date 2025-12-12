CREATE TABLE IF NOT EXISTS responsaveis (
  id INTEGER PRIMARY KEY,
  nome TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS centros (
  id INTEGER PRIMARY KEY,
  nome TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS planos (
  id INTEGER PRIMARY KEY,
  responsavel_id INTEGER NOT NULL REFERENCES responsaveis(id),
  centro_de_custo_id INTEGER NOT NULL REFERENCES centros(id)
);

CREATE TABLE IF NOT EXISTS cobrancas (
  id INTEGER PRIMARY KEY,
  plano_de_pagamento_id INTEGER NOT NULL REFERENCES planos(id) ON DELETE CASCADE,
  valor NUMERIC(18,2) NOT NULL,
  data_vencimento DATE NOT NULL,
  metodo_pagamento TEXT NOT NULL,
  status TEXT NOT NULL,
  codigo_pagamento TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS pagamentos (
  id INTEGER PRIMARY KEY,
  cobranca_id INTEGER NOT NULL REFERENCES cobrancas(id) ON DELETE CASCADE,
  valor NUMERIC(18,2) NOT NULL,
  data_pagamento TIMESTAMP NOT NULL
);
