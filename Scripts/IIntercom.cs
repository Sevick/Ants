using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IIntercom {

    public enum ResponseCode { ACCEPT, REJECT };

    void handshakeReceiver(Ant agent1);
    void handshakeResponseReceiver(ResponseCode responseCode);


    void askForResourceReceiver();
    void exchangeWaterFoodReceiver(int amount);
    void exchangeFoodWaterReceiver(int amount);
}
