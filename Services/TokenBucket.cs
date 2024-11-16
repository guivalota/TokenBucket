using System;
namespace TokenBucketWorkerService.Services
{
    public class TokenBucket
    {
        private readonly double _rate; // Taxa de geração de tokens por segundo
        private readonly int _capacity; // Capacidade máxima do bucket
        private double _tokens; // Tokens disponíveis no bucket
        private DateTime _lastCheck; // Última verificação do bucket
        public TokenBucket(double rate, int capacity)
        {
            _rate = rate;
            _capacity = capacity;
            _tokens = capacity; // Inicia o bucket cheio
            _lastCheck = DateTime.Now;
        }

        private void AddTokens()
        {
            var now = DateTime.Now;
            var elapsedSeconds = (now - _lastCheck).TotalSeconds;
            _lastCheck = now;

            // Calcula os tokens gerados e atualiza o bucket
            _tokens = Math.Min(_capacity, _tokens + elapsedSeconds * _rate);
        }

        public bool Consume(int tokens)
        {
            AddTokens();

            if (_tokens >= tokens)
            {
                _tokens -= tokens;
                return true; // Consumo permitido
            }

            return false; // Consumo negado
        }

        public string VerifyToken()
        {
            return _tokens.ToString();
        }
    }
}