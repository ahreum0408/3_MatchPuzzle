using UnityEngine;

public class ParticleManager : MonoBehaviour {
    public GameObject clearFXPrefabs;
    public GameObject breakFXPrefabs;
    public GameObject doubleBreakFXPrefabs;
    public GameObject bombClearFXPrefabs;

    public void ClearPieceFXAt(int x, int y, int z) {
        if(clearFXPrefabs != null) {
            GameObject clearFX = Instantiate(clearFXPrefabs, new Vector3(x, y, z), Quaternion.identity);
            ParticlePlayer particlePlayer = clearFX.GetComponent<ParticlePlayer>();

            if(particlePlayer != null) {
                particlePlayer.Play();
            }
        }
    }
    public void BreakTileFXAt(int breakableValue, int x, int y, int z) {
        GameObject breakFX = null;
        ParticlePlayer particlePlayer = null;

        if(breakableValue > 1) {
            if(doubleBreakFXPrefabs != null) {
                breakFX = Instantiate(doubleBreakFXPrefabs, new Vector3(x, y, z), Quaternion.identity);
            }
        }
        else {
            if(breakFXPrefabs != null) {
                breakFX = Instantiate(breakFXPrefabs, new Vector3(x, y, z), Quaternion.identity);
            }
        }

        if(breakFX != null) {
            particlePlayer = breakFX.GetComponent<ParticlePlayer>();

            if(particlePlayer != null) {
                particlePlayer.Play();
            }
        }



        /*if(breakFXPrefabs != null && doubleBreakFXPrefabs != null) {
            if(breakableValue == 1) {
                GameObject breakFX = Instantiate(breakFXPrefabs, new Vector3(x, y, z), Quaternion.identity);
                ParticlePlayer particlePlayer = breakFX.GetComponent<ParticlePlayer>();

                if(particlePlayer != null) {
                    particlePlayer.Play();
                }
            }
            else if(breakableValue == 2) {
                GameObject doubleBreakFX = Instantiate(doubleBreakFXPrefabs, new Vector3(x, y, z), Quaternion.identity);
                ParticlePlayer particlePlayer = doubleBreakFX.GetComponent<ParticlePlayer>();

                if(particlePlayer != null) {
                    particlePlayer.Play();
                }
            }
        }*/
    }
    public void BombClearFXAt(int x, int y, int z = 0) {
        if(bombClearFXPrefabs != null) {
            GameObject bombCleatFX = Instantiate(bombClearFXPrefabs, new Vector3(x, y, z), Quaternion.identity);
            ParticlePlayer particlePlayer = bombCleatFX.GetComponent<ParticlePlayer>();

            if(particlePlayer != null) {
                particlePlayer.Play();
            }
        }
    }
}
