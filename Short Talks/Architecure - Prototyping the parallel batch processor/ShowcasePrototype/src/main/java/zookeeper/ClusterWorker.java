package zookeeper;

import core.jobs.IJob;
import core.utils.SerializatorUtil;
import org.apache.zookeeper.*;
import org.apache.zookeeper.data.Stat;

import java.io.IOException;
import java.util.List;

public class ClusterWorker implements Runnable, Watcher{

    private ZooKeeper mZookeeper;

    public ClusterWorker(String connectionString) throws Exception {
        mZookeeper = new ZooKeeper(connectionString, 3000, null);
        ensurePath("/FooDistComputing");
        ensurePath("/FooDistComputing/Locks");
        ensurePath("/FooDistComputing/Tasks");
        ensurePath("/FooDistComputing/Submissions");
        mZookeeper.getChildren("/FooDistComputing/Tasks", this);
    }

    private void ensurePath(String path) throws Exception {
        if(mZookeeper.exists(path, false) == null){
            mZookeeper.create(path, null, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
        }
    }

    private String getNameFromPath(String path){
        String[] split = path.split("/");
        return split[split.length - 1];
    }

    private boolean isMinimalLock(String path, List<String> locks){
        String name = getNameFromPath(path);
        String minLock = name;
        for(String l : locks){
            minLock = minLock.compareTo(l) < 0 ? minLock : l;
        }
        return minLock.equals(name);
    }

    @Override
    public void process(WatchedEvent watchedEvent) {
        System.out.println(watchedEvent);
        try {
            for(String path : mZookeeper.getChildren("/FooDistComputing/Tasks", false)){
                String name = mZookeeper.create("/FooDistComputing/Locks/" + path +"/Lock", null, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.EPHEMERAL_SEQUENTIAL);
                List<String> locks = mZookeeper.getChildren("/FooDistComputing/Locks/" + path, false, null);
                if(mZookeeper.exists("/FooDistComputing/Tasks/" + path, false) != null){
                    if(isMinimalLock(name, locks)){
                        byte[] data = mZookeeper.getData("/FooDistComputing/Tasks/" + path, false, null);
                        SerializatorUtil.deserialize(data).run();
                        mZookeeper.delete("/FooDistComputing/Tasks/" + path, 0);
                        mZookeeper.delete(name, 0);
                    }else{
                        System.out.println("Not locked");
                        mZookeeper.delete(name, 0);
                    }
                }
            }
            mZookeeper.getChildren("/FooDistComputing/Tasks", this);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public void submit(IJob job) throws Exception {
        String path = mZookeeper.create(
                "/FooDistComputing/Tasks/Task",
                SerializatorUtil.serialize(job),
                ZooDefs.Ids.OPEN_ACL_UNSAFE,
                CreateMode.PERSISTENT_SEQUENTIAL
        );
        String name = getNameFromPath(path);
        mZookeeper.create("/FooDistComputing/Locks/" + name, null, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
    }

    @Override
    public void run() {
        while(true){
            try {
                Thread.sleep(5000);
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
    }
}
