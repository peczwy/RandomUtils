import core.jobs.impl.DummyJob;
import org.apache.zookeeper.KeeperException;
import zookeeper.ClusterWorker;
import java.io.IOException;
import java.util.Scanner;
import java.util.concurrent.Executors;

public class Main {

    public static void main(String[] args) throws Exception {
        System.out.println("Running worker");
        ClusterWorker worker = new ClusterWorker("localhost:2181");
        Executors.newSingleThreadExecutor().submit(worker);

        System.out.println("Do you want to submit the job? 0 - no, 1 - yes");
        Scanner sc = new Scanner(System.in);
        String input = sc.nextLine();
        if("0".equals(input)){
            System.out.println("Omitting submission");
        }else{
            System.out.println("Submitting the job");
            for(int i = 0; i < 100; ++i)
                worker.submit(new DummyJob());
        }
    }

}
