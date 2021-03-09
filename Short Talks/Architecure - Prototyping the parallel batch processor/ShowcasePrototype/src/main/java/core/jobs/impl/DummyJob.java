package core.jobs.impl;

import core.jobs.IJob;

import java.util.UUID;

public class DummyJob implements IJob {

    private String mMessage;

    public DummyJob(){
        this("( ͡° ͜ʖ ͡°)");
    }

    public DummyJob(String echo){
        mMessage = echo;
    }

    public void run() {
        System.out.println("I'm sexy and I say: " + mMessage);
    }

}
