namespace DefaultNamespace {
    public class EnemyUnit : Unit {
        public EnemyUnit(float power = 1.0f) : base(UnitType.Crawler, 3 * power) {
            speed = 0.06f;
            attackInterval = 20;
            attackRange = 0.75f;
            damage *= power;
        }
    }
}